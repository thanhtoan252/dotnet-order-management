using FluentValidation;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record CreateUserCommand(CreateUserRequest Request, string CreatedBy)
    : ICommand<Result<UserDto>>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(120).Matches("^[a-zA-Z0-9._-]+$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(120);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FirstName).MaximumLength(120);
        RuleFor(x => x.LastName).MaximumLength(120);
        RuleFor(x => x.Roles).NotNull();
    }
}

public sealed class CreateUserHandler(
    IKeycloakUserService keycloak,
    ILogger<CreateUserHandler> logger)
    : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var createResult = await keycloak.CreateAsync(command.Request, ct);
        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        var id = createResult.Value;

        if (command.Request.Roles.Count > 0)
        {
            await keycloak.ReplaceUserRealmRolesAsync(id, command.Request.Roles, ct);
        }

        var user = await keycloak.GetByIdAsync(id, ct);

        logger.LogInformation("User {Username} ({Id}) created by {Actor}.", command.Request.Username, id, command.CreatedBy);

        return user!;
    }
}
