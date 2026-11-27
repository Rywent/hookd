using FluentValidation;
using Hookd.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Hookd.API.Extensions;

public static class ApiExtensions
{
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                if (context.Response.HasStarted || context.RequestAborted.IsCancellationRequested)
                {
                    throw;
                }

                var statusCode = exception switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    ConflictException => StatusCodes.Status409Conflict,
                    ValidationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                ProblemDetails problemDetails;
                if (exception is ValidationException validationException)
                {
                    var errors = validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(group => group.Key,
                            group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

                    problemDetails = new ValidationProblemDetails(errors);
                }
                else
                {
                    problemDetails = new ProblemDetails();
                }

                problemDetails.Status = statusCode;
                problemDetails.Title = ReasonPhrases.GetReasonPhrase(statusCode);
                problemDetails.Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception is ValidationException ? "See the errors property for details." : exception.Message;
                problemDetails.Instance = context.Request.Path;
                problemDetails.Type = "about:blank";
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                
                await context.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(),
                    options: null, contentType: "application/problem+json",
                    cancellationToken: context.RequestAborted);
            }
        });
    }
}
