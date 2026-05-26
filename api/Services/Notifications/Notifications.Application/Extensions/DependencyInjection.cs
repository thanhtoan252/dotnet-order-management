using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Notifications.Commands;
using Notifications.Application.Notifications.Mappers;
using Notifications.Application.Notifications.Queries;
using Notifications.Application.Templates.Commands;
using Notifications.Application.Templates.Mappers;
using Notifications.Application.Templates.Queries;
using Notifications.Application.Templates.Seeding;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Notifications.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        services.AddScoped<ICommandHandler<MarkAsReadCommand, Result>, MarkAsReadHandler>();
        services.AddScoped<ICommandHandler<MarkAllAsReadCommand, Result<int>>, MarkAllAsReadHandler>();
        services.AddScoped<ICommandHandler<SendAdminNotificationCommand, Result<NotificationResponse>>,
            SendAdminNotificationHandler>();
        services.AddScoped<ICommandHandler<UpdateTemplateCommand, Result<TemplateResponse>>, UpdateTemplateHandler>();

        services.AddScoped<IQueryHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationResponse>>,
            GetMyNotificationsHandler>();
        services.AddScoped<IQueryHandler<GetUnreadCountQuery, int>, GetUnreadCountHandler>();
        services.AddScoped<IQueryHandler<GetAllTemplatesQuery, IReadOnlyList<TemplateResponse>>,
            GetAllTemplatesHandler>();

        services.AddScoped<TemplateSeeder>();

        return services;
    }
}
