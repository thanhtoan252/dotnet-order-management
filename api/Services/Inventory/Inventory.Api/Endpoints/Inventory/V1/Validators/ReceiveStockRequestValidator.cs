using FluentValidation;
using CmdDto = Inventory.Api.Endpoints.Inventory.V1.Commands.DTOs;

namespace Inventory.Api.Endpoints.Inventory.V1.Validators;

public class ReceiveStockRequestValidator : AbstractValidator<CmdDto.ReceiveStockRequest>
{
    public ReceiveStockRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0)
            .WithMessage("Quantity must be positive.");
    }
}
