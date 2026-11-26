using FluentValidation;
using Hookd.API.BackgroundServices;
using Hookd.API.Options;
using Hookd.Application.Interfaces;
using Hookd.Application.Interfaces.Webhook;
using Hookd.Application.Mapping.Webhook;
using Hookd.Application.Services.Webhook;
using Hookd.Application.Validators.Webhook;
using Hookd.Domain.Interfaces.Webhook;
using Hookd.Persistence;
using Hookd.Persistence.Repositories.Webhook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

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

// db context
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddDbContext<HookdDbContext>(
    options =>
    {
        options.UseNpgsql(connectionString);
    });

// repositories 
builder.Services.AddScoped<IWebhookTaskRepository, WebhookTaskRepository>();
builder.Services.AddScoped<IWebhookDeliveryAttemptRepository, WebhookDeliveryAttemptRepository>();

// auto mapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<WebhookTaskProfile>();
});


// services
builder.Services.AddScoped<IWebhookTaskService, WebhookTaskService>();
builder.Services.AddScoped<IUnitOfWork>(provider => 
    provider.GetRequiredService<HookdDbContext>());

// background services
builder.Services.AddHostedService<WebhookDeliveryBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
