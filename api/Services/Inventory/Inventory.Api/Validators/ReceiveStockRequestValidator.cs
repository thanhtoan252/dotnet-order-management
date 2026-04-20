using FluentValidation;
using Inventory.Application.Items.Commands;

namespace Inventory.Api.Validators;

public class ReceiveStockRequestValidator : AbstractValidator<ReceiveStockRequest>
{
    public ReceiveStockRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0)
            .WithMessage("Quantity must be positive.");
    }
}
