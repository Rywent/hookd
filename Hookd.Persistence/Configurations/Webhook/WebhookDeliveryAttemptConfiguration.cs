using Hookd.Domain.Models.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hookd.Persistence.Configurations.Webhook;

public class WebhookDeliveryAttemptConfiguration : IEntityTypeConfiguration<WebhookDeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<WebhookDeliveryAttempt> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(x => x.WebhookTaskId)
            .IsRequired();

        builder.Property(x => x.AttemptNumber)
            .IsRequired();

        builder.Property(x => x.StatusCode);

        builder.Property(x => x.IsSuccess)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.ResponseBody)
            .HasMaxLength(4000);

        builder.Property(x => x.StartedAt)
            .IsRequired();
        
        builder.HasIndex(x => x.WebhookTaskId);
        
        builder.HasIndex(x => new { x.WebhookTaskId, x.AttemptNumber });
    }
}