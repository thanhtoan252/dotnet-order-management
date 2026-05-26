using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record CreateUserCommand(CreateUserInput Input, string CreatedBy)
    : ICommand<Result<User>>;

public sealed class CreateUserHandler(
    IKeycloakUserService keycloak,
    ILogger<CreateUserHandler> logger)
    : ICommandHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var createResult = await keycloak.CreateAsync(command.Input, ct);
        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        var id = createResult.Value;

        if (command.Input.Roles.Count > 0)
        {
            await keycloak.ReplaceUserRealmRolesAsync(id, command.Input.Roles, ct);
        }

        var user = await keycloak.GetByIdAsync(id, ct);

        logger.LogInformation(
            "User {Username} ({Id}) created by {Actor}.",
            command.Input.Username,
            id,
            command.CreatedBy);

        return user!;
    }
}
