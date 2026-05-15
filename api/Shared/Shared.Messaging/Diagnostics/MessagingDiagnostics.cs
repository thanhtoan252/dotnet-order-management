using System.Diagnostics;

namespace Shared.Messaging.Diagnostics;

public static class MessagingDiagnostics
{
    public const string SourceName = "Shared.Messaging.Kafka";

    public static readonly ActivitySource ActivitySource = new(SourceName);
}
