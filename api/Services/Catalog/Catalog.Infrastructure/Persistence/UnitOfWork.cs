using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Domain;
using Serilog;

namespace Catalog.Infrastructure.Persistence;

public class UnitOfWork(CatalogDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await db.SaveChangesAsync(ct);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                await action();
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                Log.Error(ex, "Transaction rolled back due to exception: {Message}", ex.Message);
                throw;
            }
        });
    }

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> action,
        CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        Result<T> result = null!;

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            try
            {
                result = await action();
                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(ct);
                }
                else
                {
                    await transaction.RollbackAsync(ct);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                Log.Error(ex, "Transaction rolled back due to exception: {Message}", ex.Message);
                throw;
            }
        });

        return result;
    }
}
