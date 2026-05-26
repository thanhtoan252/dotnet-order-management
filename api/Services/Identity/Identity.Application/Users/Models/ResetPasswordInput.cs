namespace Identity.Application.Users.Models;

public sealed record ResetPasswordInput(string Password, bool Temporary);
