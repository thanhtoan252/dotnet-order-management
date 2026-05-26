using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Application.Users.Models;

namespace Identity.Api.Endpoints.Users.V1.Mappers;

internal static class UserMapper
{
    public static UserResponse ToDto(this User user)
    {
        return new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Enabled,
            user.EmailVerified,
            user.CreatedTimestamp,
            user.Roles);
    }
}
