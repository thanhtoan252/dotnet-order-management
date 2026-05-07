using Identity.Application.Users.Models;
using Shared.Core.Domain;

namespace Identity.Application.Users.Abstractions;

public interface IKeycloakUserService
{
    Task<IReadOnlyList<UserDto>> SearchAsync(string? search, int first, int max, bool? enabled, CancellationToken ct);
    Task<int> CountAsync(string? search, CancellationToken ct);
    Task<UserDto?> GetByIdAsync(string id, CancellationToken ct);
    Task<Result<string>> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct);
    Task DeleteAsync(string id, CancellationToken ct);
    Task ResetPasswordAsync(string id, ResetPasswordRequest request, CancellationToken ct);
    Task<IReadOnlyList<string>> GetUserRealmRolesAsync(string id, CancellationToken ct);
    Task ReplaceUserRealmRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken ct);
    Task<IReadOnlyList<RealmRoleDto>> GetRealmRolesAsync(CancellationToken ct);
}
