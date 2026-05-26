using FluentValidation;
using Notifications.Api.ApiVersioning;
using Notifications.Api.Endpoints.Notifications.V1.Mappers;
using Notifications.Api.Extensions;
using Notifications.Application.Notifications.Commands;
using Notifications.Application.Notifications.Queries;
using Notifications.Application.Templates.Commands;
using Notifications.Application.Templates.Queries;
using Notifications.Domain.Enums;
using Shared.Core.CQRS;
using Shared.Web.Authentication;
using Shared.Web.Extensions;

namespace Notifications.Api.Endpoints.Notifications.V1;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewNotificationsApiVersionSet();

        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(NotificationsApiVersions.V1)
            .RequireAuthorization();

        group.MapGet("/", GetMineAsync)
            .WithName("GetMyNotifications")
            .WithSummary("List notifications visible to the current user.");

        group.MapGet("/unread-count", GetUnreadCountAsync)
            .WithName("GetUnreadCount")
            .WithSummary("Unread notification count for the current user.");

        group.MapPost("/{id:guid}/read", MarkAsReadAsync)
            .WithName("MarkNotificationAsRead")
            .WithSummary("Mark a single notification as read.");

        group.MapPost("/mark-all-read", MarkAllAsReadAsync)
            .WithName("MarkAllNotificationsAsRead")
            .WithSummary("Mark all visible unread notifications as read.");

        group.MapPost("/", SendAdminNotificationAsync)
            .WithName("SendAdminNotification")
            .WithSummary("Send a notification to a user or broadcast to everyone.")
            .RequireAuthorization("notifications:admin");

        group.MapGet("/templates", GetTemplatesAsync)
            .WithName("GetNotificationTemplates")
            .WithSummary("List all notification templates.")
            .RequireAuthorization("notifications:admin");

        group.MapPut("/templates/{id:guid}", UpdateTemplateAsync)
            .WithName("UpdateNotificationTemplate")
            .WithSummary("Update a notification template.")
            .RequireAuthorization("notifications:admin");

        return app;
    }

    private static async Task<IResult> GetMineAsync(IDispatcher dispatcher, IUserPrinciple userPrinciple,
        int page = 1, int pageSize = 20, NotificationStatus? status = null, CancellationToken ct = default)
    {
        var query = new GetMyNotificationsQuery(
            userPrinciple.UserId,
            userPrinciple.IsInRole("admin"),
            status,
            page,
            pageSize);
        var result = await dispatcher.QueryAsync(query, ct);

        return Results.Ok(result.Select(n => n.ToDto()));
    }

    private static async Task<IResult> GetUnreadCountAsync(
        IDispatcher dispatcher, IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var query = new GetUnreadCountQuery(userPrinciple.UserId, userPrinciple.IsInRole("admin"));
        var count = await dispatcher.QueryAsync(query, ct);

        return Results.Ok(new DTOs.UnreadCountResponse
        {
            UnreadCount = count
        });
    }

    private static async Task<IResult> MarkAsReadAsync(
        Guid id, IDispatcher dispatcher, IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var command = new MarkAsReadCommand(id, userPrinciple.UserId, userPrinciple.IsInRole("admin"));
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> MarkAllAsReadAsync(
        IDispatcher dispatcher, IUserPrinciple userPrinciple, CancellationToken ct)
    {
        var command = new MarkAllAsReadCommand(userPrinciple.UserId, userPrinciple.IsInRole("admin"));
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(new DTOs.MarkAllReadResponse { Updated = result.Value })
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> SendAdminNotificationAsync(DTOs.SendAdminNotificationRequest request,
        IValidator<DTOs.SendAdminNotificationRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new SendAdminNotificationCommand(request.UserId, request.Title, request.Body, request.Metadata);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/notifications/{result.Value.Id}", result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> GetTemplatesAsync(IDispatcher dispatcher, CancellationToken ct)
    {
        var templates = await dispatcher.QueryAsync(new GetAllTemplatesQuery(), ct);
        return Results.Ok(templates.Select(t => t.ToDto()));
    }

    private static async Task<IResult> UpdateTemplateAsync(Guid id, DTOs.UpdateTemplateRequest request,
        IValidator<DTOs.UpdateTemplateRequest> validator, IDispatcher dispatcher, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var command = new UpdateTemplateCommand(id, request.Title, request.BodyTemplate, request.IsActive);
        var result = await dispatcher.SendAsync(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
