using System.Net.Http;
using System.Net.Http.Headers;
using FluentValidation;
using Hookd.Application.DTOs.Requests.Webhook;

namespace Hookd.Application.Validators.Webhook;

public class WebhookTaskAddRequestValidator : AbstractValidator<WebhookTaskAddRequest>
{
    private static readonly HashSet<string> ManagedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type", "Content-Length", "Host", "Connection", "Transfer-Encoding",
        "Trailer", "Upgrade", "Keep-Alive", "TE", "Proxy-Connection",
        "Proxy-Authorization", "Proxy-Authenticate"
    };

    public WebhookTaskAddRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("EventType cannot be empty")
            .MaximumLength(100).WithMessage("EventType must be max 100 characters")
            .Matches(@"\A[a-z][a-z0-9_]*(?:\.[a-z][a-z0-9_]*)+\z")
            .WithMessage("EventType must contain dot-separated lowercase names, for example order.created");

        RuleFor(x => x.TargetUrl)
            .NotEmpty().WithMessage("TargetUrl cannot be empty")
            .MaximumLength(2048).WithMessage("TargetUrl must be max 2048 characters")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("TargetUrl must be an absolute HTTP or HTTPS URL");

        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload cannot be empty");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType cannot be empty")
            .MaximumLength(256).WithMessage("ContentType must be max 256 characters")
            .Must(value => !value.Contains('\r') && !value.Contains('\n') &&
                           MediaTypeHeaderValue.TryParse(value, out var type) &&
                           type.MediaType is not null && !type.MediaType.Contains('*') &&
                           (type.CharSet is null || string.Equals(type.CharSet.Trim('"'), "utf-8",
                               StringComparison.OrdinalIgnoreCase)))
            .WithMessage("ContentType must be a valid media type with no charset or UTF-8 charset")
            .Must(value =>
            {
                var type = MediaTypeHeaderValue.Parse(value);
                type.CharSet = "utf-8";
                return type.ToString().Length <= 256;
            })
            .WithMessage("ContentType with UTF-8 charset must be max 256 characters");

        RuleFor(x => x.Headers)
            .Must(headers => headers is null ||
                             headers.Keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() == headers.Count)
            .WithMessage("Header names must be unique ignoring case");

        RuleForEach(x => x.Headers)
            .Must(header =>
            {
                if (string.IsNullOrWhiteSpace(header.Key) || header.Value is null ||
                    header.Value.Contains('\r') || header.Value.Contains('\n') || ManagedHeaders.Contains(header.Key))
                {
                    return false;
                }

                using var request = new HttpRequestMessage();
                try
                {
                    request.Headers.Add(header.Key, header.Value);
                    return true;
                }
                catch (Exception exception) when (exception is FormatException or InvalidOperationException or ArgumentException)
                {
                    return false;
                }
            })
            .WithMessage("Headers must contain valid request headers; content and transport headers are managed separately");

        RuleFor(x => x.HttpMethod)
            .NotEmpty().WithMessage("HttpMethod cannot be empty")
            .MaximumLength(10).WithMessage("HttpMethod must be max 10 characters")
            .Must(method =>
            {
                try
                {
                    _ = new HttpMethod(method);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            })
            .WithMessage("HttpMethod must be a valid HTTP method token");

        RuleFor(x => x.MaxAttempts)
            .GreaterThanOrEqualTo(0).WithMessage("MaxAttempts cannot be negative");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey cannot be empty")
            .MaximumLength(100).WithMessage("IdempotencyKey must be max 100 characters")
            .When(x => x.IdempotencyKey is not null);

        RuleFor(x => x.ScheduleTime)
            .Must(time => !time.HasValue || time.Value.Kind == DateTimeKind.Utc)
            .WithMessage("ScheduleTime must use UTC");
    }
}
