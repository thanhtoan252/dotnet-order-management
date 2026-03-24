using OrderManagement.Domain.Common;

namespace OrderManagement.Application.Common.Interfaces;

/// <summary>
///     Unit of Work — wraps execution strategy + transaction.
///     Application Services use this to commit atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Saves all pending changes inside a strategy-aware transaction.
    ///     Works correctly with EnableRetryOnFailure.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    ///     Executes an action inside an explicit transaction with retry support.
    ///     Use this for multi-step operations that must be atomic.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default);

    /// <summary>
    ///     Result-aware variant: rolls back without committing when the action returns a failure.
    /// </summary>
    Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> action, CancellationToken ct = default);
}