using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Shared.Core.CQRS;

namespace Identity.Application.Users.Queries;

public sealed record GetRealmRolesQuery : IQuery<IReadOnlyList<RealmRole>>;

public sealed class GetRealmRolesHandler(IKeycloakUserService keycloak)
    : IQueryHandler<GetRealmRolesQuery, IReadOnlyList<RealmRole>>
{
    public Task<IReadOnlyList<RealmRole>> HandleAsync(GetRealmRolesQuery query, CancellationToken ct)
    {
        return keycloak.GetRealmRolesAsync(ct);
    }
}
