using Shared.Core.Domain;

namespace Catalog.Domain;

public static class DomainErrors
{
    public static class Product
    {
        public static Error NotFound(Guid id)
        {
            return new Error("Product.NotFound", $"Product {id} not found.", ErrorType.NotFound);
        }

        public static Error SkuAlreadyExists(string sku)
        {
            return new Error("Product.SkuAlreadyExists", $"SKU '{sku}' already exists.", ErrorType.Conflict);
        }

        public static readonly Error InvalidName =
            new("Product.InvalidName", "Product name cannot be empty.");

        public static readonly Error InvalidSku =
            new("Product.InvalidSku", "SKU cannot be empty.");

        public static readonly Error InvalidPrice =
            new("Product.InvalidPrice", "Price is required.");
    }
}
