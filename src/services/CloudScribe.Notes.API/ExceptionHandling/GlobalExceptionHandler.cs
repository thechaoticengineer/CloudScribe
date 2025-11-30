using CloudScribe.Notes.API.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CloudScribe.Notes.API.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred: {ExceptionMessage}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred on the server.",
            Instance = httpContext.Request.Path
        };
        
        switch (exception)
        {
            // 1. Błędy Domenowe (Biznesowe) -> Logujemy jako WARNING (nie zaśmiecamy błędów)
            case DomainException domainEx:
                logger.LogWarning(domainEx, "Domain error: {Message}", domainEx.Message);
                
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad request";
                problemDetails.Detail = domainEx.Message;
                break;

            default:
                logger.LogError(exception, "Critical error: {Message}", exception.Message);
                break;
        }
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}