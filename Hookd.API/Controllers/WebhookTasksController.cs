using Hookd.Application.DTOs.Requests.Webhook;
using Hookd.Application.DTOs.Responses.Webhook;
using Hookd.Application.Interfaces.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace Hookd.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class WebhookTasksController(IWebhookTaskService webhookTaskService) : ControllerBase
{
    // GET api/webhooktasks/id
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WebhookTaskResponse>> GetById(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var webhookTask = await webhookTaskService.GetByIdAsync(id, ct);
        return Ok(webhookTask);
    }
    // POST: api/WebhookTasks/
    [HttpPost]
    public async Task<ActionResult<WebhookTaskResponse>> CreateWebhookTask(WebhookTaskAddRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        
        var response = await webhookTaskService.AddAsync(request, ct);
        
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}