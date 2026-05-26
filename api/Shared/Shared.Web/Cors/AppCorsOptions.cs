namespace Shared.Web.Cors;

public sealed class AppCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
