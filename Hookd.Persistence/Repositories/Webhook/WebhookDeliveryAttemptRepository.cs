using Hookd.Domain.Interfaces.Webhook;
using Hookd.Domain.Models.Webhook;
using Microsoft.EntityFrameworkCore;

namespace Hookd.Persistence.Repositories.Webhook;

// Stores delivery attempts in the shared context. The caller saves changes.
public class WebhookDeliveryAttemptRepository(HookdDbContext dbContext) : IWebhookDeliveryAttemptRepository
{
    public async Task<WebhookDeliveryAttempt?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.WebhookDeliveryAttempts
            .FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<WebhookDeliveryAttempt>> GetByWebhookTaskIdAsync(Guid webhookTaskId,
        CancellationToken ct = default)
    {
        return await dbContext.WebhookDeliveryAttempts
            .AsNoTracking()
            .Where(x => x.WebhookTaskId == webhookTaskId)
            .OrderBy(x => x.AttemptNumber)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);
    }

    public async Task<WebhookDeliveryAttempt> AddAsync(WebhookDeliveryAttempt attempt, CancellationToken ct = default)
    {
        await dbContext.WebhookDeliveryAttempts.AddAsync(attempt, ct);
        return attempt;
    }

    public async Task<int> UpdateAsync(WebhookDeliveryAttempt attempt, CancellationToken ct = default)
    {
        var existingAttempt = await dbContext.WebhookDeliveryAttempts.FindAsync([attempt.Id], ct);
        if (existingAttempt is null)
        {
            return 0;
        }

        // Copy scalar fields only; leave the parent task unchanged.
        dbContext.Entry(existingAttempt).CurrentValues.SetValues(attempt);
        return 1;
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var attempt = await dbContext.WebhookDeliveryAttempts.FindAsync([id], ct);
        if (attempt is null)
        {
            return 0;
        }

        dbContext.WebhookDeliveryAttempts.Remove(attempt);
        return 1;
    }
}
