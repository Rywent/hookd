using Hookd.Application.DTOs.Requests.Webhook;
using Hookd.Application.DTOs.Responses.Webhook;
using Hookd.Domain.Models.Webhook;

namespace Hookd.Application.Interfaces.Webhook;

public interface IWebhookTaskService
{
    Task RecoverInterruptedTasksAsync(CancellationToken ct = default);
    Task<WebhookTaskResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<WebhookTaskResponse>> GetReadyToProcessAsync(int batchSize, CancellationToken ct = default);

    Task<int> ProcessTasksAsync(int batchSize, CancellationToken ct = default);
    Task<WebhookDeliveryResultResponse> SendWebhookAsync(WebhookTask webhookTask, CancellationToken ct = default);
    
    Task<WebhookTaskResponse> AddAsync(WebhookTaskAddRequest request, CancellationToken ct = default);
}
