namespace Hookd.Application.DTOs.Requests.Webhook;

public class WebhookTaskAddRequest
{
    public string EventType { get; set; } = string.Empty;
    
    public string TargetUrl { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public Dictionary<string, string>? Headers { get; set; }

    public string? IdempotencyKey { get; set; }

    public string HttpMethod { get; set; } = "POST";

    public int MaxAttempts { get; set; }
    
    public DateTime? ScheduleTime { get; set; }
}
