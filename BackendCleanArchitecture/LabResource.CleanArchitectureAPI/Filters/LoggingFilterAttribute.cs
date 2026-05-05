using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LabResource.CleanApi.Filters;

public class LoggingFilterAttribute : IAsyncActionFilter
{
    private readonly ILogger<LoggingFilterAttribute> _logger;

    public LoggingFilterAttribute(ILogger<LoggingFilterAttribute> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionName = context.ActionDescriptor.RouteValues["action"] ?? "UnknownAction";
        var controllerName = context.ActionDescriptor.RouteValues["controller"] ?? "UnknownController";
        var featureName = $"{controllerName}.{actionName}";

        var userEmail = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

        var requestData = context.ActionArguments;

        _logger.LogInformation("[AUDIT] User '{User}' initiated {Feature} with data: {@Request}", userEmail, featureName, requestData);

        var resultContext = await next();

        if (resultContext.Exception == null)
        {
            _logger.LogInformation("[AUDIT] User '{User}' successfully completed {Feature}.", userEmail, featureName);
        }
    }
}