using Identity.Application.Users.Models;
using Shared.Core.Domain;

namespace Identity.Application.Users.Abstractions;

public interface IKeycloakUserService
{
    Task<IReadOnlyList<User>> SearchAsync(string? search, int first, int max, bool? enabled, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
    Task<User?> GetByIdAsync(string id, CancellationToken ct);
    Task<Result<string>> CreateAsync(CreateUserInput input, CancellationToken ct);
    Task UpdateAsync(string id, UpdateUserInput input, CancellationToken ct);
    Task DeleteAsync(string id, CancellationToken ct);
    Task ResetPasswordAsync(string id, ResetPasswordInput input, CancellationToken ct);
    Task<IReadOnlyList<string>> GetUserRealmRolesAsync(string id, CancellationToken ct);
    Task ReplaceUserRealmRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken ct);
    Task<IReadOnlyList<RealmRole>> GetRealmRolesAsync(CancellationToken ct);
}
