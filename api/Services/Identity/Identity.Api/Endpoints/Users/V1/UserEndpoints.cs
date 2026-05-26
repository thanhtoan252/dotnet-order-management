using FluentValidation;
using Identity.Api.ApiVersioning;
using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Api.Endpoints.Users.V1.Mappers;
using Identity.Api.Extensions;
using Identity.Application.Users.Commands;
using Identity.Application.Users.Queries;
using Shared.Core.CQRS;
using Shared.Web.Authentication;
using Shared.Web.Extensions;

namespace Identity.Api.Endpoints.Users.V1;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewIdentityApiVersionSet();

        var group = app.MapGroup("/api/identity/users")
            .WithTags("Identity")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(IdentityApiVersions.V1)
            .RequireAuthorization("identity:manage");

        group.MapGet("/", GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("List Keycloak users with optional search/pagination");

        group.MapGet("/count", CountUsersAsync)
            .WithName("CountUsers")
            .WithSummary("Total user count for pagination");

        group.MapGet("/realm-roles", GetRealmRolesAsync)
            .WithName("GetRealmRoles")
            .WithSummary("List all realm roles");

        group.MapGet("/{id}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get a single user by id (with role mappings)");

        group.MapGet("/{id}/roles", GetUserRolesAsync)
            .WithName("GetUserRoles")
            .WithSummary("Get a user's realm role mappings");

        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Create a new user");

        group.MapPut("/{id}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithSummary("Update user profile and enabled state");

        group.MapDelete("/{id}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Delete a user");

        group.MapPost("/{id}/reset-password", ResetPasswordAsync)
            .WithName("ResetUserPassword")
            .WithSummary("Reset a user's password");

        group.MapPut("/{id}/roles", AssignRolesAsync)
            .WithName("AssignUserRoles")
            .WithSummary("Replace a user's realm role mappings");

        return app;
    }

    private static async Task<IResult> GetUsersAsync(IDispatcher dispatcher, string? search,
        int first = 0, int max = 50, bool? enabled = null, CancellationToken ct = default)
    {
        var users = await dispatcher.QueryAsync(new GetUsersQuery(search, first, max, enabled), ct);
        return Results.Ok(users.Select(u => u.ToDto()));
    }

    private static async Task<IResult> CountUsersAsync(IDispatcher dispatcher, string? search, CancellationToken ct)
    {
        var count = await dispatcher.QueryAsync(new CountUsersQuery(search), ct);
        return Results.Ok(new UserCountResponse(count));
    }

    private static async Task<IResult> GetRealmRolesAsync(IDispatcher dispatcher, CancellationToken ct)
    {
        var roles = await dispatcher.QueryAsync(new GetRealmRolesQuery(), ct);
        return Results.Ok(roles.Select(r => r.ToDto()));
    }

    private static async Task<IResult> GetUserByIdAsync(string id, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetUserByIdQuery(id), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> GetUserRolesAsync(string id, IDispatcher dispatcher, CancellationToken ct)
    {
        var roles = await dispatcher.QueryAsync(new GetUserRolesQuery(id), ct);
        return Results.Ok(roles);
    }

    private static async Task<IResult> CreateUserAsync(CreateUserRequest request,
        IValidator<CreateUserRequest> validator, IDispatcher dispatcher, IUserPrinciple userPrinciple,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new CreateUserCommand(request.ToInput(), userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/identity/users/{result.Value.Id}", result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> UpdateUserAsync(string id, UpdateUserRequest request,
        IValidator<UpdateUserRequest> validator, IDispatcher dispatcher, IUserPrinciple userPrinciple,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new UpdateUserCommand(id, request.ToInput(), userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> DeleteUserAsync(string id, IDispatcher dispatcher,
        IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var command = new DeleteUserCommand(id, userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> ResetPasswordAsync(string id, ResetPasswordRequest request,
        IValidator<ResetPasswordRequest> validator, IDispatcher dispatcher, IUserPrinciple userPrinciple,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new ResetPasswordCommand(id, request.ToInput(), userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> AssignRolesAsync(string id, AssignRolesRequest request,
        IValidator<AssignRolesRequest> validator, IDispatcher dispatcher, IUserPrinciple userPrinciple,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new AssignRolesCommand(id, request.ToInput(), userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
