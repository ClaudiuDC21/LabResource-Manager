using System.Security.Claims;
using MediatR;

namespace LabResource.VerticalApi.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var featureName = typeof(TRequest).DeclaringType?.Name ?? typeof(TRequest).Name;
        var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[AUDIT] User '{User}' initiated {Feature} with data: {@Request}", userEmail, featureName, request);
        }

        var response = await next(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[AUDIT] User '{User}' successfully completed {Feature}.", userEmail, featureName);
        }

        return response;
    }
}