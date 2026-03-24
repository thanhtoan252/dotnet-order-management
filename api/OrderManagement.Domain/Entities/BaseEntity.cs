namespace OrderManagement.Domain.Entities;

/// <summary>
///     Base entity with audit, soft-delete, and optimistic concurrency (RowVersion).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// <summary>SQL Server ROWVERSION — optimistic concurrency token. Configured via EF Fluent API.</summary>
    public byte[]? RowVersion { get; set; }
}