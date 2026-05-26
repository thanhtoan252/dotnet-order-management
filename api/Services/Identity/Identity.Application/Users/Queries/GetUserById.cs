using Identity.Application.Common;
using Identity.Application.Users.Abstractions;
using Identity.Application.Users.Models;
using Shared.Core.CQRS;
using Shared.Core.Domain;

namespace Identity.Application.Users.Queries;

public sealed record GetUserByIdQuery(string UserId) : IQuery<Result<User>>;

public sealed class GetUserByIdHandler(IKeycloakUserService keycloak)
    : IQueryHandler<GetUserByIdQuery, Result<User>>
{
    public async Task<Result<User>> HandleAsync(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await keycloak.GetByIdAsync(query.UserId, ct);
        if (user is null)
        {
            return IdentityErrors.User.NotFound(query.UserId);
        }

        return user;
    }
}
