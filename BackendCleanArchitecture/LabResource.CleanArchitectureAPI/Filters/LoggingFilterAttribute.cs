using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

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

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[AUDIT] User '{User}' initiated {Feature} with data: {@Request}", userEmail, featureName, requestData);
        }

        var resultContext = await next();

        if (resultContext.Exception == null)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("[AUDIT] User '{User}' successfully completed {Feature}.", userEmail, featureName);
            }
        }
    }
}