using System.Net;
using System.Text.Json;
using FluentValidation;
using Sales.Application.Handlers.Sales;
using Sales.Domain.Entities;

namespace Sales.Api.Middleware;

public class GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {ErrorMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        HttpStatusCode statusCode;

        object errorResponsePayload;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                string detailMessage;
                if (validationException.Errors != null && validationException.Errors.Any())
                {
                    detailMessage = string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage));
                }
                else
                {
                    detailMessage = validationException.Message;
                }

                errorResponsePayload = new
                {
                    type = "ValidationFailure",
                    error = "Validation Failed",
                    detail = detailMessage
                };
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorResponsePayload = new
                {
                    type = "NotFound",
                    error = "Resource Not Found",
                    detail = notFoundException.Message
                };
                break;

            case DomainException domainException:
                statusCode = HttpStatusCode.BadRequest;
                errorResponsePayload = new
                {
                    type = "DomainRuleViolation",
                    error = "Business Rule Violation",
                    detail = domainException.Message
                };
                break;

            case ArgumentException argumentException:
                statusCode = HttpStatusCode.BadRequest;
                errorResponsePayload = new
                {
                    type = "InvalidArgument",
                    error = "Invalid Argument Provided",
                    detail = argumentException.Message
                };
                break;
            
            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponsePayload = new
                {
                    type = "InternalServerError",
                    error = "An unexpected error occurred.",
                    detail = "An internal server error has occurred. Please try again later."
                };
                break;
        }

         context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(errorResponsePayload, jsonOptions);

        return context.Response.WriteAsync(jsonResponse);
    }
}