using Hookd.Domain.Models.Webhook;

namespace Hookd.Application.Exceptions;

// The same key was used with different task creation parameters.
public class IdempotencyConflictException : ConflictException
{
    public string IdempotencyKey { get; }

    public IdempotencyConflictException(string idempotencyKey)
        : base(nameof(WebhookTask), nameof(WebhookTask.IdempotencyKey),
            "The idempotency key was already used with different task parameters.")
    {
        IdempotencyKey = idempotencyKey;
    }
}
