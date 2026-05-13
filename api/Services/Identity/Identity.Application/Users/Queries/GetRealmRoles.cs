using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Shared.Core.CQRS;

namespace Identity.Application.Users.Queries;

public sealed record GetRealmRolesQuery : IQuery<IReadOnlyList<RealmRoleDto>>;

public sealed class GetRealmRolesHandler(IKeycloakUserService keycloak)
    : IQueryHandler<GetRealmRolesQuery, IReadOnlyList<RealmRoleDto>>
{
    public Task<IReadOnlyList<RealmRoleDto>> HandleAsync(GetRealmRolesQuery query, CancellationToken ct)
    {
        return keycloak.GetRealmRolesAsync(ct);
    }
}
