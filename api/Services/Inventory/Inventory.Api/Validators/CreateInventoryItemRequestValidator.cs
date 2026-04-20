using FluentValidation;
using Inventory.Application.Items.Commands;

namespace Inventory.Api.Validators;

public class CreateInventoryItemRequestValidator : AbstractValidator<CreateInventoryItemRequest>
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
