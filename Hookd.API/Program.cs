using FluentValidation;
using Hookd.API.Options;
using Hookd.Application.Validators.Webhook;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddOptions<WebhookDeliveryOptions>()
    .Bind(builder.Configuration.GetSection(WebhookDeliveryOptions.SectionName))
    .Validate(settings => settings.BatchSize > 0, "BatchSize must be greater than zero.")
    .Validate(settings => settings.IdleDelay > TimeSpan.Zero, "IdleDelay must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddValidatorsFromAssemblyContaining<WebhookTaskAddRequestValidator>();
builder.Services.AddHttpClient(Options.DefaultName)
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
