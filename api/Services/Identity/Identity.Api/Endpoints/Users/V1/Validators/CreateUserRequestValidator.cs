using FluentValidation;
using Identity.Api.Endpoints.Users.V1.DTOs;

namespace Identity.Api.Endpoints.Users.V1.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(120).Matches("^[a-zA-Z0-9._-]+$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(120);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FirstName).MaximumLength(120);
        RuleFor(x => x.LastName).MaximumLength(120);
        RuleFor(x => x.Roles).NotNull();
    }
}
