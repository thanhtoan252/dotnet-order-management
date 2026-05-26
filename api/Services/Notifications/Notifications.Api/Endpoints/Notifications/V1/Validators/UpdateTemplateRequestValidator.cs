using FluentValidation;

namespace Notifications.Api.Endpoints.Notifications.V1.Validators;

public sealed class UpdateTemplateRequestValidator : AbstractValidator<DTOs.UpdateTemplateRequest>
{
    public UpdateTemplateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BodyTemplate).NotEmpty().MaximumLength(2000);
    }
}
