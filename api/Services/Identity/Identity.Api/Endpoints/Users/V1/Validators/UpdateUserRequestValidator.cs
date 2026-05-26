using FluentValidation;
using Identity.Api.Endpoints.Users.V1.DTOs;

namespace Identity.Api.Endpoints.Users.V1.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FirstName).MaximumLength(120);
        RuleFor(x => x.LastName).MaximumLength(120);
    }
}
