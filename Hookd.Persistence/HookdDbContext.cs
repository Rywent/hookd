using Hookd.Domain.Models.Webhook;
using Microsoft.EntityFrameworkCore;

namespace Hookd.Persistence;

public class HookdDbContext(DbContextOptions<HookdDbContext> options) : DbContext(options)
{
    public DbSet<WebhookTask> WebhookTasks => Set<WebhookTask>();
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts => Set<WebhookDeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HookdDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}