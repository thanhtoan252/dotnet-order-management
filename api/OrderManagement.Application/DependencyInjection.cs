using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Services;

namespace OrderManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<OrderService>();
        services.AddScoped<ProductService>();
        return services;
    }
}