using Identity.Application.Users.Abstractions;
using Shared.Core.CQRS;

namespace Identity.Application.Users.Queries;

public sealed record GetUserRolesQuery(string UserId) : IQuery<IReadOnlyList<string>>;

public sealed class GetUserRolesHandler(IKeycloakUserService keycloak)
    : IQueryHandler<GetUserRolesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> HandleAsync(GetUserRolesQuery query, CancellationToken ct)
    {
        return keycloak.GetUserRealmRolesAsync(query.UserId, ct);
    }
}
