using System.Net;
using Hookd.Application.Enums.Webhook;

namespace Hookd.Application.DTOs.Responses.Webhook;

public class WebhookDeliveryResultResponse
{
    public HttpStatusCode? StatusCode { get; init; }
    public string? ResponseBody { get; init; }
    public string? ErrorMessage { get; init; }
    public WebhookDeliveryErrorKind ErrorKind { get; init; }
    
    public double ElapsedMilliseconds { get; init; }

    public bool IsSuccess => ErrorKind == WebhookDeliveryErrorKind.None && StatusCode.HasValue &&
                             (int)StatusCode.Value >= 200 && (int)StatusCode.Value <= 299;
}
