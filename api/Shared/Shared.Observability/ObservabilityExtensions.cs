using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.OpenTelemetry;

namespace Shared.Observability;

public static class ObservabilityExtensions
{
    private const string ServiceNamespace = "order-management";
    private const string MessagingActivitySource = "Shared.Messaging.Kafka";
    private const string DefaultOtlpEndpoint = "http://localhost:4317";

    /// <summary>
    ///     Registers OpenTelemetry tracing + metrics + log export, and configures Serilog so every
    ///     log record carries the active TraceId / SpanId. Single entry point for all API projects.
    /// </summary>
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder, string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var options = ObservabilityOptions.Create(builder, serviceName);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => ConfigureResource(resource, options))
            .WithTracing(tracing => ConfigureTracing(tracing, options))
            .WithMetrics(metrics => ConfigureMetrics(metrics, options));

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            ConfigureSerilog(loggerConfiguration, context, services, options));

        return builder;
    }

    private static void ConfigureResource(ResourceBuilder resource, ObservabilityOptions options) =>
        resource
            .AddService(options.ServiceName, serviceInstanceId: Environment.MachineName)
            .AddAttributes(new KeyValuePair<string, object>[]
            {
                new("service.namespace", ServiceNamespace),
                new("deployment.environment", options.EnvironmentName)
            });

    private static void ConfigureTracing(TracerProviderBuilder tracing, ObservabilityOptions options) =>
        tracing
            .AddSource(MessagingActivitySource)
            .AddAspNetCoreInstrumentation(instrumentation =>
            {
                instrumentation.RecordException = true;
                instrumentation.Filter = ShouldTraceHttpRequest;
            })
            .AddHttpClientInstrumentation(instrumentation => instrumentation.RecordException = true)
            .AddSqlClientInstrumentation(instrumentation => instrumentation.RecordException = true)
            .AddOtlpExporter(exporter => ConfigureOtlpExporter(exporter, options));

    private static void ConfigureMetrics(MeterProviderBuilder metrics, ObservabilityOptions options) =>
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(MessagingActivitySource)
            .AddOtlpExporter(exporter => ConfigureOtlpExporter(exporter, options));

    private static void ConfigureOtlpExporter(OtlpExporterOptions exporter, ObservabilityOptions options)
    {
        exporter.Endpoint = options.OtlpEndpoint;
        exporter.Protocol = OtlpExportProtocol.Grpc;
    }

    private static bool ShouldTraceHttpRequest(HttpContext context) =>
        !context.Request.Path.StartsWithSegments("/health") &&
        !context.Request.Path.StartsWithSegments("/metrics");

    private static void ConfigureSerilog(
        LoggerConfiguration loggerConfiguration,
        HostBuilderContext context,
        IServiceProvider services,
        ObservabilityOptions options)
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("service.name", options.ServiceName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                formatter: new JsonFormatter(),
                path: $"logs/{options.ServiceName}-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .WriteTo.OpenTelemetry(sinkOptions =>
            {
                sinkOptions.Endpoint = options.OtlpEndpoint.ToString();
                sinkOptions.Protocol = OtlpProtocol.Grpc;
                sinkOptions.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = options.ServiceName,
                    ["service.namespace"] = ServiceNamespace,
                    ["deployment.environment"] = options.EnvironmentName
                };
                sinkOptions.IncludedData =
                    IncludedData.TraceIdField |
                    IncludedData.SpanIdField |
                    IncludedData.MessageTemplateTextAttribute |
                    IncludedData.MessageTemplateMD5HashAttribute;
            });
    }

    private sealed record ObservabilityOptions(
        string ServiceName,
        Uri OtlpEndpoint,
        string EnvironmentName)
    {
        public static ObservabilityOptions Create(WebApplicationBuilder builder, string serviceName)
        {
            var resolvedServiceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? serviceName;
            var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                           ?? builder.Configuration["Otel:Endpoint"]
                           ?? DefaultOtlpEndpoint;

            return new ObservabilityOptions(
                resolvedServiceName,
                new Uri(endpoint, UriKind.Absolute),
                builder.Environment.EnvironmentName);
        }
    }
}
