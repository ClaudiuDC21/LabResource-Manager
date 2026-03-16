using LabResource.Application.Interfaces.Repositories;
using LabResource.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LabResource.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}