using FluentValidation;
using LabResource.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                break;
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

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}