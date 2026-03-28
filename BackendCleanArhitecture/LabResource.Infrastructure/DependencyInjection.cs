using LabResource.Application.Interfaces.Repositories;
using LabResource.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LabResource.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILabAssetRepository, LabAssetRepository>();

        return services;
    }
}