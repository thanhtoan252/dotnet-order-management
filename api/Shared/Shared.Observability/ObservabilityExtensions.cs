using Microsoft.AspNetCore.Builder;
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
    /// <summary>
    ///     Registers OpenTelemetry tracing + metrics + log export, and configures Serilog so every
    ///     log record carries the active TraceId / SpanId. Single entry point for all API projects.
    /// </summary>
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                           ?? builder.Configuration["Otel:Endpoint"]
                           ?? "http://localhost:4317";

        var resolvedName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? serviceName;
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(rb => rb
                .AddService(resolvedName, serviceInstanceId: Environment.MachineName)
                .AddAttributes(new KeyValuePair<string, object>[]
                {
                    new("service.namespace", "order-management"),
                    new("deployment.environment", environmentName)
                }))
            .WithTracing(t => t
                .AddSource("Shared.Messaging.Kafka")
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                    o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health")
                                   && !ctx.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddSqlClientInstrumentation(o =>
                {
                    // db.statement capture is now controlled by the OTEL_DOTNET_EXPERIMENTAL_SQLCLIENT_
                    // ENABLE_TRACE_DB_STATEMENT_TEXT env var (set in docker-compose for Development).
                    o.RecordException = true;
                })
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter("Shared.Messaging.Kafka")
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }));

        builder.Host.UseSerilog((ctx, services, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("service.name", resolvedName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                formatter: new JsonFormatter(),
                path: $"logs/{resolvedName}-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .WriteTo.OpenTelemetry(opts =>
            {
                opts.Endpoint = otlpEndpoint;
                opts.Protocol = OtlpProtocol.Grpc;
                opts.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = resolvedName,
                    ["service.namespace"] = "order-management",
                    ["deployment.environment"] = environmentName
                };
                opts.IncludedData =
                    IncludedData.TraceIdField |
                    IncludedData.SpanIdField |
                    IncludedData.MessageTemplateTextAttribute |
                    IncludedData.MessageTemplateMD5HashAttribute;
            }));

        return builder;
    }
}
