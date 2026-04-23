using Order.Domain.Entities;
using Shared.Core.ValueObjects;

namespace Order.UnitTests.Helpers;

public static class TestData
{
    public static OrderAggregate CreateOrder()
    {
        var address = Address.Create("Street", "City", "Province", "12345");
        var order = OrderAggregate.Create(Guid.NewGuid(), address).Value;
        return order;
    }

    public static List<OrderAggregate> GetOrders()
    {
        return new List<OrderAggregate> { CreateOrder() };
    }
}
