using Hookd.API.Options;
using Hookd.Application.Interfaces.Webhook;
using Microsoft.Extensions.Options;

namespace Hookd.API.BackgroundServices;

public class WebhookDeliveryBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<WebhookDeliveryOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var settings = options.Value;

        while (!ct.IsCancellationRequested)
        {
            var processedCount = await RunBatchAsync(settings.BatchSize, ct);

            if (processedCount == 0)
            {
                await Task.Delay(settings.IdleDelay, ct);
            }
        }
    }

    private async Task<int> RunBatchAsync(int batchSize, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService<IWebhookTaskService>();

        var processedCount = await service.ProcessTasksAsync(batchSize, ct);
        
        return processedCount;
    }
}
