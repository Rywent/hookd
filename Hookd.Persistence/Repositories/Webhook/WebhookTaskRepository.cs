using Hookd.Domain.Enums.Webhook;
using Hookd.Domain.Interfaces.Webhook;
using Hookd.Domain.Models.Webhook;
using Microsoft.EntityFrameworkCore;

namespace Hookd.Persistence.Repositories.Webhook;

// Stores webhook tasks in the shared context. The caller saves changes.
public class WebhookTaskRepository(HookdDbContext dbContext) : IWebhookTaskRepository
{
    public async Task<WebhookTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.WebhookTasks
            .FindAsync([id], ct);
    }

    public async Task<WebhookTask?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return await dbContext.WebhookTasks
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task<IReadOnlyList<WebhookTask>> GetReadyToProcessAsync(int batchSize, DateTime utcNow,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        // Keep tracking enabled for the caller's status changes.
        // The first attempt uses ScheduleTime; retries use NextAttemptAt.
        return await dbContext.WebhookTasks
            .AsTracking()
            .Where(x => x.AttemptCount < x.MaxAttempts)
            .Where(x =>
                (x.Status == HookStatus.Pending &&
                 (x.ScheduleTime == null || x.ScheduleTime <= utcNow)) ||
                (x.Status == HookStatus.Retrying &&
                 x.NextAttemptAt != null && x.NextAttemptAt <= utcNow))
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<WebhookTask> AddAsync(WebhookTask task, CancellationToken ct = default)
    {
        var createdTask = await dbContext.WebhookTasks.AddAsync(task, ct);
        return createdTask.Entity;
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await dbContext.WebhookTasks.FindAsync([id], ct);
        if (task is null)
        {
            return 0;
        }

        dbContext.WebhookTasks.Remove(task);
        return 1;
    }
}
