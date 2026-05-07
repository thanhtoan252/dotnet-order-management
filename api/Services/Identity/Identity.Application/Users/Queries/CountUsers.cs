using Identity.Application.Users.Abstractions;
using Shared.Core.CQRS;

namespace Identity.Application.Users.Queries;

public sealed record CountUsersQuery(string? Search) : IQuery<int>;

public sealed class CountUsersHandler(IKeycloakUserService keycloak) : IQueryHandler<CountUsersQuery, int>
{
    public Task<int> HandleAsync(CountUsersQuery query, CancellationToken ct)
    {
        return keycloak.CountAsync(query.Search, ct);
    }
}
