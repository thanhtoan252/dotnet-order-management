using System.Security.Claims;
using FluentValidation;
using Order.Api.Extensions;
using Order.Application.Orders.Commands;
using Order.Application.Orders.Queries;
using Shared.Core.CQRS;

namespace Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary("Place a new order");

        group.MapGet("/", GetAllOrdersAsync)
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
            .WithSummary("Soft-delete an order. Publishes event to restore stock if Pending.")
            .RequireAuthorization("order:delete");

        return app;
    }

    private static async Task<IResult> PlaceOrderAsync(PlaceOrderRequest request,
        IValidator<PlaceOrderRequest> validator, IDispatcher dispatcher,
        HttpContext httpContext, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await dispatcher.SendAsync(new PlaceOrderCommand(request, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Created($"/api/orders/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> GetOrderAsync(Guid id, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.QueryAsync(new GetOrderByIdQuery(id), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> GetAllOrdersAsync(IDispatcher dispatcher, int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var orders = await dispatcher.QueryAsync(new GetAllOrdersQuery(page, pageSize), ct);
        return TypedResults.Ok(orders);
    }

    private static async Task<IResult> GetCustomerOrdersAsync(Guid customerId, IDispatcher dispatcher, int page = 1,
        int pageSize = 20, CancellationToken ct = default)
    {
        var orders = await dispatcher.QueryAsync(new GetCustomerOrdersQuery(customerId, page, pageSize), ct);
        return TypedResults.Ok(orders);
    }

    private static async Task<IResult> ConfirmOrderAsync(Guid id, IDispatcher dispatcher,
        HttpContext httpContext, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new ConfirmOrderCommand(id, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> ShipOrderAsync(Guid id, IDispatcher dispatcher,
        HttpContext httpContext, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new ShipOrderCommand(id, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> CancelOrderAsync(Guid id, CancelOrderRequest request, IDispatcher dispatcher,
        HttpContext httpContext, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(
            new CancelOrderCommand(id, request.Reason, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> DeliverOrderAsync(Guid id, IDispatcher dispatcher,
        HttpContext httpContext, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new DeliverOrderCommand(id, GetUsername(httpContext.User)), ct);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> DeleteOrderAsync(Guid id, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new DeleteOrderCommand(id), ct);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.NoContent();
    }

    private static string GetUsername(ClaimsPrincipal user)
    {
        return user.FindFirstValue("preferred_username")
               ?? user.FindFirstValue("sub")
               ?? "unknown";
    }
}
