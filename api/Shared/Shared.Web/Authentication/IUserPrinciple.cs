using System.Security.Claims;

namespace Shared.Web.Authentication;

public interface IUserPrinciple
{
    ClaimsPrincipal Principal { get; }

    bool IsAuthenticated { get; }

    Guid UserId { get; }

    string Username { get; }

    bool IsInRole(string role);
}
