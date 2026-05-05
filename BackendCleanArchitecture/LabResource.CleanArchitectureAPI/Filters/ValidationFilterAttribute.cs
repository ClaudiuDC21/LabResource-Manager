using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LabResource.CleanApi.Filters;

public class ValidationFilterAttribute : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilterAttribute(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values.Where(v => v != null))
        {
            var argumentType = argument!.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Failed",
                        Detail = "One or more validation errors occurred.",
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions["errors"] = validationResult.Errors
                        .Select(e => new { e.PropertyName, e.ErrorMessage });

                    context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails);
                    return;
                }
            }
        }

        await next();
    
    }
}