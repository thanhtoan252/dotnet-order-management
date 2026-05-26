using ApiGateway.Application.Handlers;

namespace ApiGateway.Application;

internal static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginHandler>();

        return services;
    }
}
