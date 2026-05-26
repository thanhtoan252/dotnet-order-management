using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Application.Users.Models;

namespace Identity.Api.Endpoints.Users.V1.Mappers;

internal static class UpdateUserMapper
{
    public static UpdateUserInput ToInput(this UpdateUserRequest request)
    {
        return new UpdateUserInput(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Enabled);
    }
}
