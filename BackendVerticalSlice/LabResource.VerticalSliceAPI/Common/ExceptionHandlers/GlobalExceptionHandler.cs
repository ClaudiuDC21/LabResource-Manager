using LabResource.VerticalApi.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.VerticalApi.Common.ExceptionHandlers;

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
            case NotFoundException notFoundEx:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message;
                break;

            case BadRequestException badRequestEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = badRequestEx.Message;
                break;

            case ConflictException conflictEx:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "State Conflict";
                problemDetails.Detail = conflictEx.Message;
                break;

            case AlreadyExistsException existsEx:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Resource Already Exists";
                problemDetails.Detail = existsEx.Message;
                break;

            case ForbiddenAccessException forbiddenEx:
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Title = "Forbidden Access";
                problemDetails.Detail = forbiddenEx.Message;
                break;

            default:
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "An internal server error occurred.";
                problemDetails.Detail = exception.Message;
                break;
        }

        // 4. Return the structured response to the client
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}