using Identity.Api.Endpoints.Users.V1.DTOs;
using Identity.Application.Users.Models;

namespace Identity.Api.Endpoints.Users.V1.Mappers;

internal static class ResetPasswordMapper
{
    public static ResetPasswordInput ToInput(this ResetPasswordRequest request)
    {
        return new ResetPasswordInput(request.Password, request.Temporary);
    }
}
