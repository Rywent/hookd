namespace Hookd.Domain.Enums.Webhook;

public enum HookStatus
{
    Pending,
    InProgress,
    Retrying,
    Succeeded,
    Failed
}