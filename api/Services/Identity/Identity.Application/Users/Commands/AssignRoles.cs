using FluentValidation;
using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record AssignRolesCommand(string UserId, AssignRolesRequest Request, string ActorUsername)
    : ICommand<Result>;

public sealed class AssignRolesCommandValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.Roles).NotNull();
        RuleForEach(x => x.Roles).NotEmpty().MaximumLength(120);
    }
}

public sealed class AssignRolesHandler(
    IKeycloakUserService keycloak,
    ILogger<AssignRolesHandler> logger)
    : ICommandHandler<AssignRolesCommand, Result>
{
    public async Task<Result> HandleAsync(AssignRolesCommand command, CancellationToken ct)
    {
        var existing = await keycloak.GetByIdAsync(command.UserId, ct);
        if (existing is null)
        {
            return IdentityErrors.User.NotFound(command.UserId);
        }

        var available = await keycloak.GetRealmRolesAsync(ct);
        var availableNames = available.Select(r => r.Name).ToHashSet(StringComparer.Ordinal);

        var unknown = command.Request.Roles.FirstOrDefault(r => !availableNames.Contains(r));
        if (unknown is not null)
        {
            return IdentityErrors.Role.NotFound(unknown);
        }

        await keycloak.ReplaceUserRealmRolesAsync(command.UserId, command.Request.Roles, ct);

        logger.LogInformation("Roles for user {Id} replaced by {Actor}. Roles=[{Roles}].",
            command.UserId, command.ActorUsername, string.Join(',', command.Request.Roles));

        return Result.Success();
    }
}
