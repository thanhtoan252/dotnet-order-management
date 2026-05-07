namespace Identity.Application.Users.Models;

public sealed record ResetPasswordRequest(string Password, bool Temporary);
