using Microsoft.AspNetCore.Mvc;
using SatoshiForge.Application.Exceptions;

namespace SatoshiForge.Api.Extensions;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddProblemDetailsConfiguration(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var exception = context.HttpContext.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?
                    .Error;

                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                if (exception is null)
                    return;

                switch (exception)
                {
                    case ApplicationValidationException validationException:
                        context.ProblemDetails.Title = "Validation error";
                        context.ProblemDetails.Type = "https://httpstatuses.com/400";
                        context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
                        context.ProblemDetails.Detail = "One or more validation errors occurred.";
                        context.ProblemDetails.Extensions["errors"] = validationException.Errors;
                        break;

                    case ConflictException:
                        context.ProblemDetails.Title = "Conflict";
                        context.ProblemDetails.Type = "https://httpstatuses.com/409";
                        context.ProblemDetails.Status = StatusCodes.Status409Conflict;
                        break;

                    case UnauthorizedException:
                        context.ProblemDetails.Title = "Unauthorized";
                        context.ProblemDetails.Type = "https://httpstatuses.com/401";
                        context.ProblemDetails.Status = StatusCodes.Status401Unauthorized;
                        break;

                    case NotFoundException:
                        context.ProblemDetails.Title = "Resource not found";
                        context.ProblemDetails.Type = "https://httpstatuses.com/404";
                        context.ProblemDetails.Status = StatusCodes.Status404NotFound;
                        break;

                    default:
                        context.ProblemDetails.Title = "Internal server error";
                        context.ProblemDetails.Type = "https://httpstatuses.com/500";
                        context.ProblemDetails.Status = StatusCodes.Status500InternalServerError;
                        break;
                }

                context.ProblemDetails.Detail = exception.Message;
            };
        });

        return services;
    }
}