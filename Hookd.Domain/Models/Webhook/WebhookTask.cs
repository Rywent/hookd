using Hookd.Domain.Enums.Webhook;

namespace Hookd.Domain.Models.Webhook;

public class WebhookTask
{

    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    
    public string TargetUrl { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? Headers { get; set; }
    public string? IdempotencyKey { get; set; }
    public string HttpMethod { get; set; } = "POST";
    
    public HookStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    
    public int? LastStatusCode { get; set; }
    public string? LastError { get; set; } = string.Empty;
    public string? LastResponseBody { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SuccessAt { get; set; }
    public DateTime? FailureAt { get; set; }
    
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? ScheduleTime { get; set; }
    
    public List<WebhookDeliveryAttempt> Attempts { get; set; } = new();
    
}