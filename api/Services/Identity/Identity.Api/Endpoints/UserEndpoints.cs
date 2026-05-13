using System.Security.Claims;
using FluentValidation;
using Identity.Api.Extensions;
using Identity.Application.Users.Commands;
using Identity.Application.Users.Models;
using Identity.Application.Users.Queries;
using Shared.Core.CQRS;

namespace Identity.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity/users")
            .WithTags("Identity")
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
        return TypedResults.Ok(users);
    }

    private static async Task<IResult> CountUsersAsync(IDispatcher dispatcher, string? search, CancellationToken ct)
    {
        var count = await dispatcher.QueryAsync(new CountUsersQuery(search), ct);
        return TypedResults.Ok(new { count });
    }

    private static async Task<IResult> GetRealmRolesAsync(IDispatcher dispatcher, CancellationToken ct)
    {
        var roles = await dispatcher.QueryAsync(new GetRealmRolesQuery(), ct);
        return TypedResults.Ok(roles);
    }

    private static async Task<IResult> GetUserByIdAsync(string id, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetUserByIdQuery(id), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> GetUserRolesAsync(string id, IDispatcher dispatcher, CancellationToken ct)
    {
        var roles = await dispatcher.QueryAsync(new GetUserRolesQuery(id), ct);
        return TypedResults.Ok(roles);
    }

    private static async Task<IResult> CreateUserAsync(CreateUserRequest request,
        IValidator<CreateUserRequest> validator, IDispatcher dispatcher, HttpContext httpContext, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(
            new CreateUserCommand(request, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Created($"/api/identity/users/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateUserAsync(string id, UpdateUserRequest request,
        IValidator<UpdateUserRequest> validator, IDispatcher dispatcher, HttpContext httpContext, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(
            new UpdateUserCommand(id, request, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> DeleteUserAsync(string id, IDispatcher dispatcher,
        HttpContext httpContext, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new DeleteUserCommand(id, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> ResetPasswordAsync(string id, ResetPasswordRequest request,
        IValidator<ResetPasswordRequest> validator, IDispatcher dispatcher, HttpContext httpContext, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(
            new ResetPasswordCommand(id, request, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> AssignRolesAsync(string id, AssignRolesRequest request,
        IValidator<AssignRolesRequest> validator, IDispatcher dispatcher, HttpContext httpContext, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(
            new AssignRolesCommand(id, request, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }

    private static string GetUsername(ClaimsPrincipal user)
    {
        return user.FindFirstValue("preferred_username")
               ?? user.FindFirstValue("sub")
               ?? "unknown";
    }
}
