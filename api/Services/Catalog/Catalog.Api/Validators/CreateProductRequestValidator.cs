using Catalog.Application.Products.Commands;
using FluentValidation;

namespace Catalog.Api.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be positive.");
        RuleFor(x => x.InitialStockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Initial stock quantity cannot be negative.")
            .When(x => x.InitialStockQuantity.HasValue);
    }
}
