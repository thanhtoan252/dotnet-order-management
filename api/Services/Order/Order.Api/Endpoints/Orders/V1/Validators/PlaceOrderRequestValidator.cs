using FluentValidation;
using CmdDto = Order.Api.Endpoints.Orders.V1.Commands.DTOs;

namespace Order.Api.Endpoints.Orders.V1.Validators;

public class PlaceOrderRequestValidator : AbstractValidator<CmdDto.PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.ShippingAddress).NotNull();
        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress.Street).NotEmpty();
            RuleFor(x => x.ShippingAddress.City).NotEmpty();
            RuleFor(x => x.ShippingAddress.Province).NotEmpty();
            RuleFor(x => x.ShippingAddress.ZipCode).NotEmpty();
        });

        RuleFor(x => x.Lines).NotEmpty().WithMessage("Order must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
