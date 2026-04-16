using Shared.Core.Domain;

namespace Order.Domain;

public static class DomainErrors
{
    public static class Order
    {
        public static readonly Error EmptyItems =
            new("Order.EmptyItems", "Cannot confirm an order with no items.");

        public static readonly Error InvalidCustomer =
            new("Order.InvalidCustomer", "CustomerId cannot be empty.");

        public static readonly Error CurrencyMismatch =
            new("Order.CurrencyMismatch", "Cannot mix different currencies in the same order.");

        public static readonly Error InvalidQuantity =
            new("Order.InvalidQuantity", "Quantity must be greater than zero.");

        public static Error NotFound(Guid id)
        {
            return new Error("Order.NotFound", $"Order {id} not found.");
        }

        public static Error InvalidState(string state, string operation)
        {
            return new Error("Order.InvalidState", $"Cannot '{operation}' on order in '{state}' state.");
        }

        public static Error ProductsNotFound(IEnumerable<Guid> ids)
        {
            return new Error("Order.ProductsNotFound", $"Products not found: {string.Join(", ", ids)}");
        }

        public static Error InsufficientStock(string details)
        {
            return new Error("Order.InsufficientStock", $"Stock check failed: {details}");
        }
    }
}
