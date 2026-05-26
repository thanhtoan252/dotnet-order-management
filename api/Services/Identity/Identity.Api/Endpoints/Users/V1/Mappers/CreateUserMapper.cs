using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Application.Users.Models;

namespace Identity.Api.Endpoints.Users.V1.Mappers;

internal static class CreateUserMapper
{
    public static CreateUserInput ToInput(this CreateUserRequest request)
    {
        return new CreateUserInput(
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            request.Enabled,
            request.Password,
            request.TemporaryPassword,
            request.Roles);
    }
}
