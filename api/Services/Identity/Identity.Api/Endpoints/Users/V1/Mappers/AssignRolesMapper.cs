using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Application.Users.Models;

namespace Identity.Api.Endpoints.Users.V1.Mappers;

internal static class AssignRolesMapper
{
    public static AssignRolesInput ToInput(this AssignRolesRequest request)
    {
        return new AssignRolesInput(request.Roles);
    }
}
