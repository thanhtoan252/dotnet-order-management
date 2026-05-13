using FluentValidation;
using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Logging;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Commands;

public sealed record ResetPasswordCommand(string UserId, ResetPasswordRequest Request, string ActorUsername)
    : ICommand<Result>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(120);
    }
}

public sealed class ResetPasswordHandler(
    IKeycloakUserService keycloak,
    ILogger<ResetPasswordHandler> logger)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken ct)
    {
        var existing = await keycloak.GetByIdAsync(command.UserId, ct);
        if (existing is null)
        {
            return IdentityErrors.User.NotFound(command.UserId);
        }

        await keycloak.ResetPasswordAsync(command.UserId, command.Request, ct);

        logger.LogInformation("Password reset for user {Id} by {Actor}. Temporary={Temporary}.",
            command.UserId, command.ActorUsername, command.Request.Temporary);

        return Result.Success();
    }
}
