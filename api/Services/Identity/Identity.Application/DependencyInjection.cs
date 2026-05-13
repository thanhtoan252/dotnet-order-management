using FluentValidation;
using Identity.Application.Users.Models;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using UserCmd = Identity.Application.Users.Commands;
using UserQry = Identity.Application.Users.Queries;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ICommandHandler<UserCmd.CreateUserCommand, Result<UserDto>>, UserCmd.CreateUserHandler>();
        services.AddScoped<ICommandHandler<UserCmd.UpdateUserCommand, Result<UserDto>>, UserCmd.UpdateUserHandler>();
        services.AddScoped<ICommandHandler<UserCmd.DeleteUserCommand, Result>, UserCmd.DeleteUserHandler>();
        services.AddScoped<ICommandHandler<UserCmd.ResetPasswordCommand, Result>, UserCmd.ResetPasswordHandler>();
        services.AddScoped<ICommandHandler<UserCmd.AssignRolesCommand, Result>, UserCmd.AssignRolesHandler>();

        services.AddScoped<IQueryHandler<UserQry.GetUsersQuery, IReadOnlyList<UserDto>>, UserQry.GetUsersHandler>();
        services.AddScoped<IQueryHandler<UserQry.CountUsersQuery, int>, UserQry.CountUsersHandler>();
        services.AddScoped<IQueryHandler<UserQry.GetUserByIdQuery, Result<UserDto>>, UserQry.GetUserByIdHandler>();
        services.AddScoped<IQueryHandler<UserQry.GetUserRolesQuery, IReadOnlyList<string>>, UserQry.GetUserRolesHandler>();
        services.AddScoped<IQueryHandler<UserQry.GetRealmRolesQuery, IReadOnlyList<RealmRoleDto>>, UserQry.GetRealmRolesHandler>();

        return services;
    }
}
