using Microsoft.Extensions.DependencyInjection;
using Shared.Web.Authentication;

namespace Shared.Web.Extensions;

public static class UserPrincipleExtensions
{
    public static IServiceCollection AddUserPrinciple(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserPrinciple, HttpContextUserPrinciple>();

        return services;
    }
}
