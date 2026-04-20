namespace ApiGateway.Infrastructure.ReverseProxy;

internal static class ReverseProxyExtensions
{
    public static IServiceCollection AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }
}
