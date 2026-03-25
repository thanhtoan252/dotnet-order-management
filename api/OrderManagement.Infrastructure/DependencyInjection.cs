using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Common.Interfaces;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sql.CommandTimeout(30);
                    sql.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName);
                }));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}