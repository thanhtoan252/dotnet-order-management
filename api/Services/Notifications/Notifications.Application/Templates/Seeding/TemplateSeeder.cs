using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notifications.Application.Abstractions;

namespace Notifications.Application.Templates.Seeding;

public sealed class TemplateSeeder(INotificationDbContext db, ILogger<TemplateSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        var existingTypes = await db.Templates.Select(t => t.Type).ToListAsync(ct);
        var existing = new HashSet<Domain.Enums.NotificationType>(existingTypes);

        var inserted = 0;
        foreach (var template in DefaultTemplates.All())
        {
            if (existing.Contains(template.Type))
            {
                continue;
            }

            db.Templates.Add(template);
            inserted++;
        }

        if (inserted > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} notification template(s).", inserted);
        }
    }
}
