using FluentValidation;
using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record UpdateUserCommand(string UserId, UpdateUserRequest Request, string UpdatedBy)
    : ICommand<Result<UserDto>>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FirstName).MaximumLength(120);
        RuleFor(x => x.LastName).MaximumLength(120);
    }
}

public sealed class UpdateUserHandler(
    IKeycloakUserService keycloak,
    ILogger<UpdateUserHandler> logger)
    : ICommandHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(UpdateUserCommand command, CancellationToken ct)
    {
        var existing = await keycloak.GetByIdAsync(command.UserId, ct);
        if (existing is null)
        {
            return IdentityErrors.User.NotFound(command.UserId);
        }

        await keycloak.UpdateAsync(command.UserId, command.Request, ct);

        var refreshed = await keycloak.GetByIdAsync(command.UserId, ct);

        logger.LogInformation("User {Id} updated by {Actor}.", command.UserId, command.UpdatedBy);

        return refreshed!;
    }
}
