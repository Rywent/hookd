namespace Hookd.API.Options;

public class WebhookDeliveryOptions
{
    public const string SectionName = "WebhookDelivery";
    public int BatchSize { get; set; } = 5;
    public TimeSpan IdleDelay { get; set; } = TimeSpan.FromSeconds(5);
}
