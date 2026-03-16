using LabResource.Application.Interfaces.Services;
using LabResource.Application.Services;
using Microsoft.Extensions.DependencyInjection;


namespace LabResource.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}