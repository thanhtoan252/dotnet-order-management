using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record UpdateUserCommand(string UserId, UpdateUserInput Input, string UpdatedBy)
    : ICommand<Result<User>>;

public sealed class UpdateUserHandler(
    IKeycloakUserService keycloak,
    ILogger<UpdateUserHandler> logger)
    : ICommandHandler<UpdateUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(UpdateUserCommand command, CancellationToken ct)
    {
        var existing = await keycloak.GetByIdAsync(command.UserId, ct);
        if (existing is null)
        {
            return IdentityErrors.User.NotFound(command.UserId);
        }

        await keycloak.UpdateAsync(command.UserId, command.Input, ct);

        var refreshed = await keycloak.GetByIdAsync(command.UserId, ct);

        logger.LogInformation("User {Id} updated by {Actor}.", command.UserId, command.UpdatedBy);

        return refreshed!;
    }
}
