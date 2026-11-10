namespace Hookd.Domain.Models.Webhook;

public class WebhookDeliveryAttempt
{
    public Guid Id { get; set; }

    public Guid WebhookTaskId { get; set; }
    public WebhookTask WebhookTask { get; set; } = null!;

    public int AttemptNumber { get; set; }

    public int? StatusCode { get; set; }
    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }
    public string? ResponseBody { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? DurationMs { get; set; }
}