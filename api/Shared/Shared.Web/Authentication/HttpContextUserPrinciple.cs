using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Shared.Web.Authentication;

internal sealed class HttpContextUserPrinciple(IHttpContextAccessor httpContextAccessor) : IUserPrinciple
{
    public ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var sub = Principal.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    public string Username =>
        Principal.FindFirstValue("preferred_username")
        ?? Principal.FindFirstValue("sub")
        ?? "unknown";

    public bool IsInRole(string role) => Principal.IsInRole(role);
}
