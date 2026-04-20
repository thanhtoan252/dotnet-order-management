using ApiGateway.Application.Contracts;
using ApiGateway.Application.Handlers;

namespace ApiGateway.Application.Endpoints;

internal static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, LoginHandler handler, CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(request, ct);

            return response is null ? Results.Unauthorized() : Results.Ok(response);
        })
        .AllowAnonymous()
        .RequireCors("CorsPolicy")
        .WithTags("Auth");

        return app;
    }
}
