using Hookd.Domain.Models.Webhook;

namespace Hookd.Domain.Interfaces.Webhook;

// Delivery attempt storage. Changes are saved by the caller.
// UpdateAsync and DeleteAsync return 0 when missing, otherwise 1.
public interface IWebhookDeliveryAttemptRepository
{
    Task<List<WebhookDeliveryAttempt>> GetUnfinishedForInProgressTasksAsync(CancellationToken ct = default);
    Task<WebhookDeliveryAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // History is ordered by attempt number and returned without tracking.
    Task<IReadOnlyList<WebhookDeliveryAttempt>> GetByWebhookTaskIdAsync(Guid webhookTaskId, CancellationToken ct = default);

    Task<WebhookDeliveryAttempt> AddAsync(WebhookDeliveryAttempt attempt, CancellationToken ct = default);

    Task<int> UpdateAsync(WebhookDeliveryAttempt attempt, CancellationToken ct = default);

    Task<int> DeleteAsync(Guid id, CancellationToken ct = default);
}
