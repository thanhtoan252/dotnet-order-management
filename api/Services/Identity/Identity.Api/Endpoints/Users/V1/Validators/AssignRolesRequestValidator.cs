using FluentValidation;
using Identity.Api.Endpoints.Users.V1.DTOs;

namespace Identity.Api.Endpoints.Users.V1.Validators;

public sealed class AssignRolesRequestValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRolesRequestValidator()
    {
        RuleFor(x => x.Roles).NotNull();
        RuleForEach(x => x.Roles).NotEmpty().MaximumLength(120);
    }
}
