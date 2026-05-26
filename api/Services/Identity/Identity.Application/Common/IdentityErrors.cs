using Shared.Core.Domain;

namespace Identity.Application.Common;

public static class IdentityErrors
{
    public static class User
    {
        public static Error NotFound(string id)
            => new("User.NotFound", $"User '{id}' was not found.", ErrorType.NotFound);
        public static Error Conflict(string message) => new("User.Conflict", message, ErrorType.Conflict);
        public static Error UpstreamFailure(string message)
            => new("User.UpstreamFailure", message, ErrorType.BadGateway);

        public static Error UsernameImmutable()
            => new("User.InvalidState", "Username cannot be changed.", ErrorType.Conflict);
    }

    public static class Role
    {
        public static Error NotFound(string name)
            => new("Role.NotFound", $"Realm role '{name}' does not exist.", ErrorType.NotFound);
    }
}
