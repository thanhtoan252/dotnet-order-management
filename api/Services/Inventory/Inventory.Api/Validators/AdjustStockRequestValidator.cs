using FluentValidation;
using Inventory.Application.Items.Commands;

namespace Inventory.Api.Validators;

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.OnHand).GreaterThanOrEqualTo(0)
            .WithMessage("OnHand cannot be negative.");
    }
}
