using FluentValidation;
using CmdDto = Catalog.Api.Endpoints.Products.V1.Commands.DTOs;

namespace Catalog.Api.Endpoints.Products.V1.Validators;

public class CreateProductRequestValidator : AbstractValidator<CmdDto.CreateProductRequest>
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
