using Hookd.Domain.Enums.Webhook;

namespace Hookd.Domain.Models.Webhook;

// A webhook request and its delivery state. All timestamps are UTC.
public class WebhookTask
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    
    public string TargetUrl { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json; charset=utf-8";

    // Request headers stored as a JSON object.
    public string? Headers { get; set; }

    // Deduplication key for task creation.
    public string? IdempotencyKey { get; set; }
    public string HttpMethod { get; set; } = "POST";
    
    public HookStatus Status { get; set; }

    public int AttemptCount { get; set; }

    // Includes the first delivery attempt.
    public int MaxAttempts { get; set; }
    
    public int? LastStatusCode { get; set; }
    public string? LastError { get; set; } = string.Empty;
    public string? LastResponseBody { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DateTime? SuccessAt { get; set; }
    public DateTime? FailureAt { get; set; }
    
    // Earliest retry time. Null means no retry is scheduled.
    public DateTime? NextAttemptAt { get; set; }

    // Earliest time for the first attempt. Null means it can start immediately.
    public DateTime? ScheduleTime { get; set; }
    
    public List<WebhookDeliveryAttempt> Attempts { get; set; } = new();
    
}
