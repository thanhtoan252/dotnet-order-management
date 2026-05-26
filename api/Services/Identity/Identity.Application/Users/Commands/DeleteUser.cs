using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record DeleteUserCommand(string UserId, string DeletedBy) : ICommand<Result>;

public sealed class DeleteUserHandler(
    IKeycloakUserService keycloak,
    ILogger<DeleteUserHandler> logger)
    : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteUserCommand command, CancellationToken ct)
    {
        var existing = await keycloak.GetByIdAsync(command.UserId, ct);
        if (existing is null)
        {
            return IdentityErrors.User.NotFound(command.UserId);
        }

        await keycloak.DeleteAsync(command.UserId, ct);

        logger.LogInformation(
            "User {Id} ({Username}) deleted by {Actor}.",
            command.UserId,
            existing.Username,
            command.DeletedBy);

        return Result.Success();
    }
}
