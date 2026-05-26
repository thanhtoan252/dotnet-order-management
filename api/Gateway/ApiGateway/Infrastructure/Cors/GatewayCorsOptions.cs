namespace ApiGateway.Infrastructure.Cors;

internal sealed class GatewayCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
