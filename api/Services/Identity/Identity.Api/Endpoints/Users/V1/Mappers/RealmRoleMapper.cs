using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Application.Users.Models;

namespace Identity.Api.Endpoints.Users.V1.Mappers;

internal static class RealmRoleMapper
{
    public static RealmRoleResponse ToDto(this RealmRole role)
    {
        return new RealmRoleResponse(role.Id, role.Name, role.Description);
    }
}
