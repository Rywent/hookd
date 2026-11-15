using Hookd.Domain.Models.Webhook;

namespace Hookd.Domain.Interfaces.Webhook;

// Webhook task storage. Changes are saved by the caller.
// DeleteAsync returns 0 when missing, or 1 when marked for deletion.
public interface IWebhookTaskRepository
{
    Task<WebhookTask?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<WebhookTask?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);

    // Returns tracked tasks that are due, oldest first. Does not claim them for a worker.
    Task<IReadOnlyList<WebhookTask>> GetReadyToProcessAsync(int batchSize, DateTime utcNow,
        CancellationToken ct = default);

    Task<WebhookTask> AddAsync(WebhookTask task, CancellationToken ct = default);

    Task<int> DeleteAsync(Guid id, CancellationToken ct = default);
}
