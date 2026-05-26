using Microsoft.Extensions.Logging;
using Notifications.Application.Abstractions;
using Notifications.Application.Notifications.Mappers;
using Notifications.Application.Realtime;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Notifications.Application.Notifications.Commands;

public sealed record SendAdminNotificationCommand(Guid? TargetUserId, string Title, string Body, string? Metadata)
    : ICommand<Result<NotificationResponse>>;

public sealed class SendAdminNotificationHandler(
    INotificationDbContext db,
    INotificationPusher pusher,
    ILogger<SendAdminNotificationHandler> logger)
    : ICommandHandler<SendAdminNotificationCommand, Result<NotificationResponse>>
{
    public async Task<Result<NotificationResponse>> HandleAsync(SendAdminNotificationCommand command,
        CancellationToken ct = default)
    {
        Notification notification;

        if (command.TargetUserId is { } userId)
        {
            notification = Notification.ForUser(userId, NotificationType.AdminBroadcast, command.Title, command.Body,
                metadata: command.Metadata);
        }
        else
        {
            notification = Notification.Broadcast(command.Title, command.Body, command.Metadata);
        }

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        var response = notification.ToResponse();

        if (command.TargetUserId is { } targetId)
        {
            await pusher.PushToUserAsync(targetId, response, ct);
            logger.LogInformation(
                "Admin notification {NotificationId} sent to user {UserId}",
                notification.Id,
                targetId);
        }
        else
        {
            await pusher.PushToAllAsync(response, ct);
            logger.LogInformation("Admin broadcast notification {NotificationId} sent to all users", notification.Id);
        }

        return response;
    }
}
