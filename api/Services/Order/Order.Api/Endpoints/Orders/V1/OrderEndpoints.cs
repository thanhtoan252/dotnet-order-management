using FluentValidation;
using Order.Api.ApiVersioning;
using Order.Api.Endpoints.Orders.V1.Mappers;
using Order.Api.Extensions;
using Order.Application.Orders.Commands;
using Order.Application.Orders.Queries;
using Shared.Core.CQRS;
using Shared.Web.Authentication;
using Shared.Web.Extensions;
using CmdDto = Order.Api.Endpoints.Orders.V1.Commands.DTOs;

namespace Order.Api.Endpoints.Orders.V1;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewOrderApiVersionSet();

        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(OrderApiVersions.V1)
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

    private static async Task<IResult> PlaceOrderAsync(CmdDto.PlaceOrderRequest request,
        IValidator<CmdDto.PlaceOrderRequest> validator, IDispatcher dispatcher,
        IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new PlaceOrderCommand(request.ToInput(), userPrinciple.Username, userPrinciple.UserId);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/orders/{result.Value.Id}", result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> GetOrderAsync(Guid id, IDispatcher dispatcher, CancellationToken ct)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await dispatcher.QueryAsync(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToQueryDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> GetAllOrdersAsync(IDispatcher dispatcher, int page = 1, int pageSize = 100,
        CancellationToken ct = default)
    {
        var query = new GetAllOrdersQuery(page, pageSize);
        var orders = await dispatcher.QueryAsync(query, ct);

        return Results.Ok(orders.Select(o => o.ToQueryDto()));
    }

    private static async Task<IResult> GetCustomerOrdersAsync(Guid customerId, IDispatcher dispatcher, int page = 1,
        int pageSize = 20, CancellationToken ct = default)
    {
        var query = new GetCustomerOrdersQuery(customerId, page, pageSize);
        var orders = await dispatcher.QueryAsync(query, ct);
        return Results.Ok(orders.Select(o => o.ToQueryDto()));
    }

    private static async Task<IResult> ConfirmOrderAsync(Guid id, IDispatcher dispatcher,
        IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var command = new ConfirmOrderCommand(id, userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> ShipOrderAsync(Guid id, IDispatcher dispatcher,
        IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var command = new ShipOrderCommand(id, userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> CancelOrderAsync(
        Guid id, CmdDto.CancelOrderRequest request, IDispatcher dispatcher, IUserPrinciple userPrinciple,
        CancellationToken ct)
    {
        var command = new CancelOrderCommand(id, request.Reason, userPrinciple.Username, userPrinciple.UserId);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> DeliverOrderAsync(Guid id, IDispatcher dispatcher,
        IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var command = new DeliverOrderCommand(id, userPrinciple.Username);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToCommandDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> DeleteOrderAsync(Guid id, IDispatcher dispatcher, CancellationToken ct)
    {
        var command = new DeleteOrderCommand(id);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
