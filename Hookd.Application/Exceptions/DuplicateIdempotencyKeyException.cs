using Hookd.Domain.Models.Webhook;

namespace Hookd.Application.Exceptions;

// Raised by persistence when another task already owns the key.
public class DuplicateIdempotencyKeyException : ConflictException
{
    public string IdempotencyKey { get; }

    public DuplicateIdempotencyKeyException(string idempotencyKey, Exception innerException)
        : base(nameof(WebhookTask), nameof(WebhookTask.IdempotencyKey),
            "A task with this idempotency key already exists.", innerException)
    {
        IdempotencyKey = idempotencyKey;
    }
}
