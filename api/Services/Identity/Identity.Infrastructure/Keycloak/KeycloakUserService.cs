using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Microsoft.Extensions.Options;
using Refit;
using Shared.Core.Domain;

namespace Identity.Infrastructure.Keycloak;

public sealed class KeycloakUserService(IKeycloakAdminApi api, IOptions<KeycloakOptions> options)
    : IKeycloakUserService
{
    private readonly string _realm = options.Value.Realm;

    public async Task<IReadOnlyList<User>> SearchAsync(
        string? search, int first, int max, bool? enabled, CancellationToken ct)
    {
        var users = await api.SearchUsersAsync(_realm, search, first, max, enabled, briefRepresentation: true, ct);

        var roleTasks = users
            .Where(u => !string.IsNullOrEmpty(u.Id))
            .Select(async u =>
            {
                var roles = await api.GetUserRealmRolesAsync(_realm, u.Id!, ct);
                return (u.Id!, (IReadOnlyList<string>)roles
                    .Where(r => r.Name is not null)
                    .Select(r => r.Name!)
                    .ToList());
            })
            .ToList();

        var rolesById = (await Task.WhenAll(roleTasks))
            .ToDictionary(t => t.Item1, t => t.Item2, StringComparer.Ordinal);

        return users
            .Select(u => MapUser(u, u.Id is not null && rolesById.TryGetValue(u.Id, out var roles) ? roles : []))
            .ToList();
    }

    public Task<int> CountAsync(string? search, CancellationToken ct)
    {
        return api.CountUsersAsync(_realm, search, ct);
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        try
        {
            var user = await api.GetUserAsync(_realm, id, ct);
            if (user is null)
            {
                return null;
            }

            var roles = await api.GetUserRealmRolesAsync(_realm, id, ct);
            var roleNames = roles.Where(r => r.Name is not null).Select(r => r.Name!).ToList();

            return MapUser(user, roleNames);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Result<string>> CreateAsync(CreateUserInput input, CancellationToken ct)
    {
        var representation = new KeycloakUserRepresentation
        {
            Username = input.Username,
            Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email,
            FirstName = input.FirstName,
            LastName = input.LastName,
            Enabled = input.Enabled,
            EmailVerified = false,
            Credentials =
            [
                new KeycloakCredentialRepresentation
                {
                    Type = "password",
                    Value = input.Password,
                    Temporary = input.TemporaryPassword
                }
            ]
        };

        try
        {
            var response = await api.CreateUserAsync(_realm, representation, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return IdentityErrors.User.Conflict("Username or email already exists.");
            }

            response.EnsureSuccessStatusCode();

            var location = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(location))
            {
                return IdentityErrors.User.UpstreamFailure("Keycloak did not return a Location header.");
            }

            return Result<string>.Success(location[(location.LastIndexOf('/') + 1)..]);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return IdentityErrors.User.Conflict("Username or email already exists.");
        }
    }

    public async Task UpdateAsync(string id, UpdateUserInput input, CancellationToken ct)
    {
        var representation = new KeycloakUserRepresentation
        {
            Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email,
            FirstName = input.FirstName,
            LastName = input.LastName,
            Enabled = input.Enabled
        };

        await api.UpdateUserAsync(_realm, id, representation, ct);
    }

    public Task DeleteAsync(string id, CancellationToken ct)
    {
        return api.DeleteUserAsync(_realm, id, ct);
    }

    public Task ResetPasswordAsync(string id, ResetPasswordInput input, CancellationToken ct)
    {
        var credential = new KeycloakCredentialRepresentation
        {
            Type = "password",
            Value = input.Password,
            Temporary = input.Temporary
        };

        return api.ResetPasswordAsync(_realm, id, credential, ct);
    }

    public async Task<IReadOnlyList<string>> GetUserRealmRolesAsync(string id, CancellationToken ct)
    {
        var roles = await api.GetUserRealmRolesAsync(_realm, id, ct);

        return roles.Where(r => r.Name is not null).Select(r => r.Name!).ToList();
    }

    public async Task ReplaceUserRealmRolesAsync(string id, IReadOnlyList<string> roleNames, CancellationToken ct)
    {
        var availableRoles = await api.GetRealmRolesAsync(_realm, ct);
        var availableByName = availableRoles
            .Where(r => r.Name is not null)
            .ToDictionary(r => r.Name!, r => r, StringComparer.Ordinal);

        var desiredSet = new HashSet<string>(roleNames, StringComparer.Ordinal);
        var current = await api.GetUserRealmRolesAsync(_realm, id, ct);
        var currentNames = current
            .Where(r => r.Name is not null)
            .Select(r => r.Name!)
            .ToHashSet(StringComparer.Ordinal);

        var toAdd = desiredSet
            .Where(n => !currentNames.Contains(n) && availableByName.ContainsKey(n))
            .Select(n => availableByName[n])
            .ToList();

        var toRemove = current
            .Where(r => r.Name is not null && !desiredSet.Contains(r.Name))
            .ToList();

        if (toAdd.Count > 0)
        {
            await api.AssignUserRealmRolesAsync(_realm, id, toAdd, ct);
        }

        if (toRemove.Count > 0)
        {
            await api.RemoveUserRealmRolesAsync(_realm, id, toRemove, ct);
        }
    }

    public async Task<IReadOnlyList<RealmRole>> GetRealmRolesAsync(CancellationToken ct)
    {
        var roles = await api.GetRealmRolesAsync(_realm, ct);

        return roles
            .Where(r => r.Id is not null && r.Name is not null)
            .Select(r => new RealmRole(r.Id!, r.Name!, r.Description))
            .ToList();
    }

    private static User MapUser(KeycloakUserRepresentation user, IReadOnlyList<string> roles)
    {
        return new User(
            Id: user.Id ?? string.Empty,
            Username: user.Username ?? string.Empty,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Enabled: user.Enabled ?? false,
            EmailVerified: user.EmailVerified ?? false,
            CreatedTimestamp: user.CreatedTimestamp,
            Roles: roles);
    }
}
