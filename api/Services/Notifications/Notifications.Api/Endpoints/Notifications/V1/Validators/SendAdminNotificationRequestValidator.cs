using FluentValidation;

namespace Notifications.Api.Endpoints.Notifications.V1.Validators;

public sealed class SendAdminNotificationRequestValidator : AbstractValidator<DTOs.SendAdminNotificationRequest>
{
    public SendAdminNotificationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}
