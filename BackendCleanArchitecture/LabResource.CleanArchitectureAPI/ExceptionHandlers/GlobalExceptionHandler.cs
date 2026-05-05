using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the exception using Serilog
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 2. Map the exception to a standardized format (RFC 7807 - Problem Details)
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        // 3. Determine the HTTP status code based on the exception type
        switch (exception)
        {
            // TODO: Uncomment these as we create the custom exceptions in the Domain layer
            // case ValidationException validationEx:
            //     problemDetails.Status = StatusCodes.Status400BadRequest;
            //     problemDetails.Title = "Validation Error";
            //     problemDetails.Detail = validationEx.Message;
            //     break;
            // case NotFoundException notFoundEx:
            //     problemDetails.Status = StatusCodes.Status404NotFound;
            //     problemDetails.Title = "Resource Not Found";
            //     problemDetails.Detail = notFoundEx.Message;
            //     break;
            default:
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "An internal server error occurred.";
                // Note: Hide the actual exception message in production for security reasons
                problemDetails.Detail = exception.Message;
                break;
        }

        // 4. Return the structured response to the client
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}