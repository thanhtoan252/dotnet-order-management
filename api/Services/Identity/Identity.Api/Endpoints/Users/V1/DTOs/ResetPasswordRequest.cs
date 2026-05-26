namespace Identity.Api.Endpoints.Users.V1.DTOs;

public sealed record ResetPasswordRequest(string Password, bool Temporary);
