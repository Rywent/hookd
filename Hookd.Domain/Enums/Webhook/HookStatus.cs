namespace Hookd.Domain.Enums.Webhook;

public enum HookStatus
{
    // Waiting for the first attempt, including tasks scheduled for later.
    Pending,

    // A worker is processing the task.
    InProgress,

    // Waiting for NextAttemptAt after an unsuccessful attempt.
    Retrying,

    // Delivery completed successfully.
    Succeeded,

    // Delivery stopped after a permanent error or after all attempts were used.
    Failed
}
