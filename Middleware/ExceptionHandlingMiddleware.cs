// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Middleware for handling exceptions and returning standardized error responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case NotFoundException notFound:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new ErrorResponse(
                    notFound.Message,
                    "NOT_FOUND",
                    (int)HttpStatusCode.NotFound
                );
                _logger.LogInformation("Not found exception: {Message}", notFound.Message);
                break;

            case ValidationException validation:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse(
                    validation.Message,
                    validation.Errors,
                    (int)HttpStatusCode.BadRequest
                );
                _logger.LogWarning("Validation exception: {Message}", validation.Message);
                break;

            case BusinessException business:
                response.StatusCode = business.HttpStatusCode;
                errorResponse = new ErrorResponse(
                    business.Message,
                    business.ErrorCode ?? "BUSINESS_ERROR",
                    business.HttpStatusCode
                );
                _logger.LogWarning("Business exception: {Message}", business.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse(
                    "An unexpected error occurred",
                    "INTERNAL_SERVER_ERROR",
                    (int)HttpStatusCode.InternalServerError
                );
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        errorResponse.TraceId = context.TraceIdentifier;
        return response.WriteAsJsonAsync(errorResponse);
    }
}
