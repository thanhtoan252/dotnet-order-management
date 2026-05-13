using Shared.Core.Domain;

namespace Identity.Application.Common;

public static class IdentityErrors
{
    public static class User
    {
        public static Error NotFound(string id) => new("User.NotFound", $"User '{id}' was not found.");
        public static Error Conflict(string message) => new("User.Conflict", message);
        public static Error UpstreamFailure(string message) => new("User.UpstreamFailure", message);
        public static Error UsernameImmutable() => new("User.InvalidState", "Username cannot be changed.");
    }

    public static class Role
    {
        public static Error NotFound(string name) => new("Role.NotFound", $"Realm role '{name}' does not exist.");
    }
}
