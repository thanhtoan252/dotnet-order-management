namespace ApiGateway.Infrastructure.Cors;

internal static class CorsExtensions
{
    public const string PolicyName = "CorsPolicy";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, GatewayCorsOptions corsOptions, IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            if (environment.IsDevelopment() || corsOptions.AllowedOrigins.Length == 0)
            {
                options.AddPolicy(PolicyName, p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }
            else
            {
                options.AddPolicy(PolicyName,
                    p => p.WithOrigins(corsOptions.AllowedOrigins).AllowAnyMethod().AllowAnyHeader());
            }
        });

        return services;
    }
}
