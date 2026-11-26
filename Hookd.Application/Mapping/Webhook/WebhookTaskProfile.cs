using AutoMapper;
using Hookd.Application.DTOs.Requests.Webhook;
using Hookd.Application.DTOs.Responses.Webhook;
using Hookd.Domain.Models.Webhook;

namespace Hookd.Application.Mapping.Webhook;

public class WebhookTaskProfile : Profile
{
    public WebhookTaskProfile()
    {
        CreateMap<WebhookTaskAddRequest, WebhookTask>()
            .ForMember(x => x.Headers, options => options.MapFrom(x => WebhookHeadersMapping.ToJson(x.Headers)))
            .ForMember(x => x.Id, options => options.Ignore())
            .ForMember(x => x.Status, options => options.Ignore())
            .ForMember(x => x.AttemptCount, options => options.Ignore())
            .ForMember(x => x.LastStatusCode, options => options.Ignore())
            .ForMember(x => x.LastError, options => options.Ignore())
            .ForMember(x => x.LastResponseBody, options => options.Ignore())
            .ForMember(x => x.CreatedAt, options => options.Ignore())
            .ForMember(x => x.UpdatedAt, options => options.Ignore())
            .ForMember(x => x.SuccessAt, options => options.Ignore())
            .ForMember(x => x.FailureAt, options => options.Ignore())
            .ForMember(x => x.NextAttemptAt, options => options.Ignore())
            .ForMember(x => x.Attempts, options => options.Ignore());

        CreateMap<WebhookTask, WebhookTaskResponse>()
            .ForMember(x => x.Headers, options => options.MapFrom(x => WebhookHeadersMapping.FromJson(x.Headers)));
    }
}
