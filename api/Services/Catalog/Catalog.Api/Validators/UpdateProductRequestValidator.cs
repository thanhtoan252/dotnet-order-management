using Catalog.Application.Products.Commands;
using FluentValidation;

namespace Catalog.Api.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive.")
            .When(x => x.Price.HasValue);
    }
}
