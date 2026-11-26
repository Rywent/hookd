using Hookd.Application.Exceptions;
using Hookd.Application.Interfaces;
using Hookd.Domain.Models.Webhook;
using Hookd.Persistence.Configurations.Webhook;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Hookd.Persistence;

public class HookdDbContext(DbContextOptions<HookdDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<WebhookTask> WebhookTasks => Set<WebhookTask>();
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts => Set<WebhookDeliveryAttempt>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await base.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
               {
                   SqlState: PostgresErrorCodes.UniqueViolation,
                   ConstraintName: WebhookTaskConfiguration.IdempotencyKeyIndexName
               })
        {
            var addedTasks = exception.Entries
                .Where(x => x.State == EntityState.Added && x.Entity is WebhookTask { IdempotencyKey: not null })
                .ToArray();
            if (addedTasks.Length != 1)
            {
                throw;
            }

            var task = (WebhookTask)addedTasks[0].Entity;
            // The failed insert must not be retried on the next save in this context.
            addedTasks[0].State = EntityState.Detached;
            throw new DuplicateIdempotencyKeyException(task.IdempotencyKey!, exception);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HookdDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
