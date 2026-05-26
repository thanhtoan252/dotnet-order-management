using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Shared.Core.CQRS;

namespace Identity.Application.Users.Queries;

public sealed record GetUsersQuery(string? Search, int First, int Max, bool? Enabled)
    : IQuery<IReadOnlyList<User>>;

public sealed class GetUsersHandler(IKeycloakUserService keycloak)
    : IQueryHandler<GetUsersQuery, IReadOnlyList<User>>
{
    public Task<IReadOnlyList<User>> HandleAsync(GetUsersQuery query, CancellationToken ct)
    {
        return keycloak.SearchAsync(query.Search, query.First, query.Max, query.Enabled, ct);
    }
}
