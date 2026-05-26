using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Application.Templates.Mappers;
using Notifications.Domain;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Notifications.Application.Templates.Commands;

public sealed record UpdateTemplateCommand(Guid TemplateId, string Title, string BodyTemplate, bool IsActive)
    : ICommand<Result<TemplateResponse>>;

public sealed class UpdateTemplateHandler(INotificationDbContext db)
    : ICommandHandler<UpdateTemplateCommand, Result<TemplateResponse>>
{
    public async Task<Result<TemplateResponse>> HandleAsync(UpdateTemplateCommand command,
        CancellationToken ct = default)
    {
        var template = await db.Templates.SingleOrDefaultAsync(t => t.Id == command.TemplateId, ct);
        if (template is null)
        {
            return DomainErrors.Template.NotFound(command.TemplateId);
        }

        template.Update(command.Title, command.BodyTemplate, command.IsActive);
        await db.SaveChangesAsync(ct);

        return template.ToResponse();
    }
}
