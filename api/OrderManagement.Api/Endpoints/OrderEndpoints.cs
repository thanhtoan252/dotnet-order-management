using System.Security.Claims;
using FluentValidation;
using OrderManagement.Api.Extensions;
using OrderManagement.Api.Mappers;
using OrderManagement.Application.Orders;
using OrderManagement.Application.Orders.Commands;
using OrderManagement.Application.Services;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary("Place a new order");

        group.MapGet("", GetAllOrdersAsync)
            .WithName("GetAllOrders")
            .WithSummary("Get all orders");

        group.MapGet("/{id:guid}", GetOrderAsync)
            .WithName("GetOrder")
            .WithSummary("Get order by ID");

        group.MapGet("/customer/{customerId:guid}", GetCustomerOrdersAsync)
            .WithName("GetCustomerOrders")
            .WithSummary("List orders for a customer");

        group.MapPost("/{id:guid}/confirm", ConfirmOrderAsync)
            .WithName("ConfirmOrder")
            .WithSummary("Confirm a pending order")
            .RequireAuthorization("order:confirm");

        group.MapPost("/{id:guid}/ship", ShipOrderAsync)
            .WithName("ShipOrder")
            .WithSummary("Mark order as shipped")
            .RequireAuthorization("order:ship");

        group.MapPost("/{id:guid}/cancel", CancelOrderAsync)
            .WithName("CancelOrder")
            .WithSummary("Cancel an order");

        group.MapPost("/{id:guid}/deliver", DeliverOrderAsync)
            .WithName("DeliverOrder")
            .WithSummary("Mark order as delivered")
            .RequireAuthorization("order:deliver");

        group.MapDelete("/{id:guid}", DeleteOrderAsync)
            .WithName("DeleteOrder")
            .WithSummary("Soft-delete an order. Restores stock if the order is still Pending.")
            .RequireAuthorization("order:delete");

        return app;
    }

    private static async Task<IResult> PlaceOrderAsync(PlaceOrderRequest request,
        IValidator<PlaceOrderRequest> validator, OrderService svc, HttpContext httpContext, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var address = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.Province,
            request.ShippingAddress.ZipCode);

        var lines = request.Lines
            .Select(l => (l.ProductId, l.Quantity))
            .ToList();

        var result = await svc.PlaceOrderAsync(
            request.CustomerId, address, lines, request.Notes, GetUsername(httpContext.User), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Created($"/api/orders/{result.Value.Id}", result.Value.ToResponse());
    }

    private static async Task<IResult> GetOrderAsync(Guid id, OrderService svc, CancellationToken ct)
    {
        var result = await svc.GetOrderAsync(id, ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value.ToResponse());
    }

    private static async Task<IResult> GetAllOrdersAsync(OrderService svc, int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var orders = await svc.GetAllOrdersAsync(page, pageSize, ct);

        return TypedResults.Ok(orders.Select(o => o.ToResponse()).ToList());
    }

    private static async Task<IResult> GetCustomerOrdersAsync(Guid customerId, OrderService svc, int page = 1,
        int pageSize = 20, CancellationToken ct = default)
    {
        var orders = await svc.GetCustomerOrdersAsync(customerId, page, pageSize, ct);

        return TypedResults.Ok(orders.Select(o => o.ToResponse()).ToList());
    }

    private static async Task<IResult> ConfirmOrderAsync(Guid id, OrderService svc, HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await svc.ConfirmOrderAsync(id, GetUsername(httpContext.User), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value.ToResponse());
    }

    private static async Task<IResult> ShipOrderAsync(Guid id, OrderService svc, HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await svc.ShipOrderAsync(id, GetUsername(httpContext.User), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value.ToResponse());
    }

    private static async Task<IResult> CancelOrderAsync(Guid id, CancelOrderRequest request, OrderService svc,
        HttpContext httpContext, CancellationToken ct)
    {
        var result = await svc.CancelOrderAsync(id, request.Reason, GetUsername(httpContext.User), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value.ToResponse());
    }

    private static async Task<IResult> DeliverOrderAsync(Guid id, OrderService svc, HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await svc.DeliverOrderAsync(id, GetUsername(httpContext.User), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value.ToResponse());
    }

    private static async Task<IResult> DeleteOrderAsync(Guid id, OrderService svc, CancellationToken ct)
    {
        var result = await svc.DeleteOrderAsync(id, ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }

    private static string GetUsername(ClaimsPrincipal user)
    {
        return user.FindFirstValue("preferred_username")
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? "unknown";
    }
}
