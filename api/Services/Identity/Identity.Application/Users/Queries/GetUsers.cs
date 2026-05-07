using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Shared.Core.CQRS;

namespace Identity.Application.Users.Queries;

public sealed record GetUsersQuery(string? Search, int First, int Max, bool? Enabled)
    : IQuery<IReadOnlyList<UserDto>>;

public sealed class GetUsersHandler(IKeycloakUserService keycloak)
    : IQueryHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    public Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersQuery query, CancellationToken ct)
    {
        return keycloak.SearchAsync(query.Search, query.First, query.Max, query.Enabled, ct);
    }
}
