using FluentValidation;
using Identity.Api.Endpoints.Users.V1.DTOs;

namespace Identity.Api.Endpoints.Users.V1.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(120);
    }
}
