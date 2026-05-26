using FluentValidation;
using CmdDto = Catalog.Api.Endpoints.Products.V1.Commands.DTOs;

namespace Catalog.Api.Endpoints.Products.V1.Validators;

public class UpdateProductRequestValidator : AbstractValidator<CmdDto.UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive.")
            .When(x => x.Price.HasValue);
    }
}
