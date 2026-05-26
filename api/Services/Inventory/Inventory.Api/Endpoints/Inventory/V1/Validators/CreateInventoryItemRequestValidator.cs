using FluentValidation;
using CmdDto = Inventory.Api.Endpoints.Inventory.V1.Commands.DTOs;

namespace Inventory.Api.Endpoints.Inventory.V1.Validators;

public class CreateInventoryItemRequestValidator : AbstractValidator<CmdDto.CreateInventoryItemRequest>
{
    public CreateInventoryItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0)
            .WithMessage("Initial quantity cannot be negative.");
    }
}
