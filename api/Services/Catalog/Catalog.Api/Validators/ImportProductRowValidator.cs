using Catalog.Application.Products.Commands;
using FluentValidation;

namespace Catalog.Api.Validators;

public sealed class ImportProductRowValidator : AbstractValidator<ImportProductsRow>
{
    public ImportProductRowValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).NotNull().GreaterThan(0).WithMessage("Price must be positive.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.InitialStockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Initial stock quantity cannot be negative.")
            .When(x => x.InitialStockQuantity.HasValue);
    }
}
