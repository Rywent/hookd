using Hookd.Domain.Models.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hookd.Persistence.Configurations.Webhook;

public class WebhookTaskConfiguration : IEntityTypeConfiguration<WebhookTask>
{
    public void Configure(EntityTypeBuilder<WebhookTask> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TargetUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.HttpMethod)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("POST");
        
        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(100);

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.Property(x => x.LastResponseBody)
            .HasMaxLength(4000);
        
        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        
        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });

        builder.HasIndex(x => x.CreatedAt);
        
        builder.HasMany(x => x.Attempts)
            .WithOne(x => x.WebhookTask)
            .HasForeignKey(x => x.WebhookTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}