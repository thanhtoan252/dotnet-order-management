using FluentValidation;
using CmdDto = Inventory.Api.Endpoints.Inventory.V1.Commands.DTOs;

namespace Inventory.Api.Endpoints.Inventory.V1.Validators;

public class AdjustStockRequestValidator : AbstractValidator<CmdDto.AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.OnHand).GreaterThanOrEqualTo(0)
            .WithMessage("OnHand cannot be negative.");
    }
}
