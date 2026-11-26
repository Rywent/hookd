namespace Hookd.Domain.Models.Webhook;

// A single delivery attempt. All timestamps are UTC.
public class WebhookDeliveryAttempt
{
    public Guid Id { get; set; }

    public Guid WebhookTaskId { get; set; }
    public WebhookTask WebhookTask { get; set; } = null!;

    // Starts at 1 within each task.
    public int AttemptNumber { get; set; }

    // Null if no HTTP response was received.
    public int? StatusCode { get; set; }
    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }
    public string? ResponseBody { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }
    public double? DurationMs { get; set; }
}
