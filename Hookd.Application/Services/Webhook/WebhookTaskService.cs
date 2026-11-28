using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using FluentValidation;
using Hookd.Application.DTOs.Requests.Webhook;
using Hookd.Application.DTOs.Responses.Webhook;
using Hookd.Application.Enums.Webhook;
using Hookd.Application.Exceptions;
using Hookd.Application.Interfaces;
using Hookd.Application.Interfaces.Webhook;
using Hookd.Application.Mapping.Webhook;
using Hookd.Domain.Enums.Webhook;
using Hookd.Domain.Interfaces.Webhook;
using Hookd.Domain.Models.Webhook;

namespace Hookd.Application.Services.Webhook;

public class WebhookTaskService(IWebhookTaskRepository taskRepository, IWebhookDeliveryAttemptRepository deliveryAttemptRepository, IMapper mapper, IUnitOfWork unitOfWork,
    IValidator<WebhookTaskAddRequest> validator, IHttpClientFactory httpClientFactory)
    : IWebhookTaskService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

    public async Task RecoverInterruptedTasksAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var attempts = await deliveryAttemptRepository.GetUnfinishedForInProgressTasksAsync(ct);
        if (attempts.Count == 0)
        {
            return;
        }

        var now = ToDatabasePrecision(DateTime.UtcNow);
        const string errorMessage = "Delivery interrupted; outcome unknown.";

        foreach (var attempt in attempts)
        {
            ct.ThrowIfCancellationRequested();
            var webhookTask = attempt.WebhookTask;
            
            attempt.FinishedAt = now;
            attempt.IsSuccess = false;
            attempt.StatusCode = null;
            attempt.DurationMs = null;
            attempt.ResponseBody = null;
            attempt.ErrorMessage = errorMessage;

            webhookTask.UpdatedAt = now;
            webhookTask.LastStatusCode = null;
            webhookTask.LastResponseBody = null;
            webhookTask.LastError = errorMessage;
            webhookTask.SuccessAt = null;
            webhookTask.FailureAt = null;
            webhookTask.NextAttemptAt = null;

            if (webhookTask.AttemptCount >= webhookTask.MaxAttempts)
            {
                webhookTask.Status = HookStatus.Failed;
                webhookTask.FailureAt = now;
            }
            else
            {
                webhookTask.Status = HookStatus.Retrying;
                webhookTask.NextAttemptAt = now.Add(RetryDelay);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
    public async Task<WebhookTaskResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var webhookTask = await taskRepository.GetByIdAsync(id, ct);
        if (webhookTask == null)
        {
            throw new NotFoundException(nameof(WebhookTask), id);
        }
        return mapper.Map<WebhookTaskResponse>(webhookTask);
    }
    private WebhookTaskResponse GetIdempotentResponse(WebhookTask existingTask, WebhookTask requestedTask)
    {
        if (existingTask.EventType != requestedTask.EventType ||
            existingTask.TargetUrl != requestedTask.TargetUrl ||
            existingTask.Payload != requestedTask.Payload ||
            !WebhookHeadersMapping.AreEqual(existingTask.Headers, requestedTask.Headers) ||
            !MediaTypeHeaderValue.Parse(existingTask.ContentType)
                .Equals(MediaTypeHeaderValue.Parse(requestedTask.ContentType)) ||
            existingTask.HttpMethod != requestedTask.HttpMethod ||
            existingTask.MaxAttempts != requestedTask.MaxAttempts ||
            existingTask.ScheduleTime != requestedTask.ScheduleTime)
        {
            throw new IdempotencyConflictException(requestedTask.IdempotencyKey!);
        }

        return mapper.Map<WebhookTaskResponse>(existingTask);
    }

    public async Task<IReadOnlyList<WebhookTaskResponse>> GetReadyToProcessAsync(int batchSize, CancellationToken ct = default)
    {
        DateTime now = ToDatabasePrecision(DateTime.UtcNow);
        var webhookTasks = await taskRepository.GetReadyToProcessAsync(batchSize, now, ct);
        return mapper.Map<IReadOnlyList<WebhookTaskResponse>>(webhookTasks);
    }
     
    public async Task<WebhookTaskResponse> AddAsync(WebhookTaskAddRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ct.ThrowIfCancellationRequested();
        await validator.ValidateAndThrowAsync(request, ct);

        var webhookTask = mapper.Map<WebhookTask>(request);
        var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
        contentType.MediaType = contentType.MediaType!.ToLowerInvariant();
        contentType.CharSet = "utf-8";
        webhookTask.ContentType = contentType.ToString();
        webhookTask.MaxAttempts = request.MaxAttempts == 0 ? 3 : request.MaxAttempts;
        webhookTask.ScheduleTime = request.ScheduleTime.HasValue
            ? ToDatabasePrecision(request.ScheduleTime.Value) : null;

        if (webhookTask.IdempotencyKey is not null)
        {
            var existingTask = await taskRepository.GetByIdempotencyKeyAsync(webhookTask.IdempotencyKey, ct);
            if (existingTask is not null)
            {
                return GetIdempotentResponse(existingTask, webhookTask);
            }
        }

        var now = ToDatabasePrecision(DateTime.UtcNow);
        webhookTask.CreatedAt = now;
        webhookTask.UpdatedAt = now;
        webhookTask.Status = HookStatus.Pending;
        webhookTask.AttemptCount = 0;

        await taskRepository.AddAsync(webhookTask, ct);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DuplicateIdempotencyKeyException exception)
            when (exception.IdempotencyKey == webhookTask.IdempotencyKey)
        {
            // Another request may have created the task after our initial lookup.
            var existingTask = await taskRepository.GetByIdempotencyKeyAsync(exception.IdempotencyKey, ct);
            if (existingTask is null)
            {
                throw;
            }

            return GetIdempotentResponse(existingTask, webhookTask);
        }

        return mapper.Map<WebhookTaskResponse>(webhookTask);
    }


    // Receives a batch and sequentially triggers the processing of each task.
    public async Task<int> ProcessTasksAsync(int batchSize, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        DateTime now = ToDatabasePrecision(DateTime.UtcNow);
        
        int processedCount = 0;
        
        var webhookTasks = await taskRepository.GetReadyToProcessAsync(batchSize, now, ct);
        
        if (webhookTasks.Count == 0)
        {
            return 0;
        }

        foreach (var webhookTask in webhookTasks)
        {
            await ProcessTaskAsync(webhookTask, ct);
            processedCount++;
            
        }
        return processedCount;
    }

    private async Task ProcessTaskAsync(WebhookTask webhookTask, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        var now = ToDatabasePrecision(DateTime.UtcNow);
        webhookTask.UpdatedAt = now;
        webhookTask.Status = HookStatus.InProgress;
        webhookTask.NextAttemptAt = null;
        webhookTask.AttemptCount++;

        WebhookDeliveryAttempt attempt = new WebhookDeliveryAttempt();
        attempt.WebhookTaskId = webhookTask.Id;
        attempt.AttemptNumber = webhookTask.AttemptCount;
        attempt.StartedAt = now;
        attempt.FinishedAt = null;
                
        await deliveryAttemptRepository.AddAsync(attempt, ct);

        await unitOfWork.SaveChangesAsync(ct);
                
        var result = await SendWebhookAsync(webhookTask, ct);
        ApplyDeliveryResult(webhookTask, attempt, result);

        await unitOfWork.SaveChangesAsync(ct);
    }

    private static void ApplyDeliveryResult(WebhookTask webhookTask, WebhookDeliveryAttempt attempt,
        WebhookDeliveryResultResponse result)
    {
        var now = ToDatabasePrecision(DateTime.UtcNow);
        var errorMessage = result.ErrorMessage;
        if (!result.IsSuccess && errorMessage is null && result.StatusCode.HasValue)
        {
            errorMessage = $"Receiver returned HTTP {(int)result.StatusCode.Value}.";
        }

        attempt.FinishedAt = now;
        attempt.DurationMs = result.ElapsedMilliseconds;
        attempt.StatusCode = (int?)result.StatusCode;
        attempt.IsSuccess = result.IsSuccess;
        attempt.ErrorMessage = TruncateForStorage(errorMessage, 2000);
        attempt.ResponseBody = TruncateForStorage(result.ResponseBody, 4000);

        webhookTask.UpdatedAt = now;
        webhookTask.LastStatusCode = attempt.StatusCode;
        webhookTask.LastError = attempt.ErrorMessage;
        webhookTask.LastResponseBody = attempt.ResponseBody;
        webhookTask.NextAttemptAt = null;
        webhookTask.SuccessAt = null;
        webhookTask.FailureAt = null;

        if (result.IsSuccess)
        {
            webhookTask.Status = HookStatus.Succeeded;
            webhookTask.SuccessAt = now;
            return;
        }

        var statusCode = (int?)result.StatusCode;
        var canRetry = result.ErrorKind is WebhookDeliveryErrorKind.Network or WebhookDeliveryErrorKind.Timeout ||
                       statusCode is 408 or 429 or >= 500 and <= 599;

        if (canRetry && webhookTask.AttemptCount < webhookTask.MaxAttempts)
        {
            webhookTask.Status = HookStatus.Retrying;
            webhookTask.NextAttemptAt = now.Add(RetryDelay);
            return;
        }

        webhookTask.Status = HookStatus.Failed;
        webhookTask.FailureAt = now;
    }

    private static string? TruncateForStorage(string? value, int maxLength)
    {
        if (value is null || value.Length <= maxLength)
        {
            return value;
        }

        // Keep UTF-16 surrogate pairs intact when truncating response text.
        var length = char.IsHighSurrogate(value[maxLength - 1]) && char.IsLowSurrogate(value[maxLength])
            ? maxLength - 1 : maxLength;
        return value[..length];
    }
    
    public async Task<WebhookDeliveryResultResponse> SendWebhookAsync(WebhookTask webhookTask, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(webhookTask);
        ct.ThrowIfCancellationRequested();

        using var httpClient = httpClientFactory.CreateClient();
        
        using var request = new HttpRequestMessage(
            new HttpMethod(webhookTask.HttpMethod),
            webhookTask.TargetUrl
        );
        
        request.Content = new StringContent(
            webhookTask.Payload,
            Encoding.UTF8,
            MediaTypeHeaderValue.Parse(webhookTask.ContentType)
        );

        foreach (var (key, value) in WebhookHeadersMapping.FromJson(webhookTask.Headers))
        {
            request.Headers.Add(key, value);
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var response = await httpClient.SendAsync(request, ct);

            stopwatch.Stop();
            
            return new WebhookDeliveryResultResponse
            {
                StatusCode = response.StatusCode,
                ResponseBody = await response.Content.ReadAsStringAsync(ct),
                ErrorKind = WebhookDeliveryErrorKind.None,
                ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds 
            };
        }
        // Caller cancellation must propagate to the worker.
        catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
        {
            stopwatch.Stop();
            
            return new WebhookDeliveryResultResponse
            {
                ErrorMessage = ex.Message,
                ErrorKind = WebhookDeliveryErrorKind.Timeout,
                ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            
            return new WebhookDeliveryResultResponse
            {
                StatusCode = ex.StatusCode,
                ErrorMessage = ex.Message,
                ErrorKind = WebhookDeliveryErrorKind.Network,
                ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds
            };
        }
    }
    

    
    // PostgreSQL timestamps have microsecond precision.
    private static DateTime ToDatabasePrecision(DateTime value)
    {
        return value.AddTicks(-(value.Ticks % TimeSpan.TicksPerMicrosecond));
    }
}
