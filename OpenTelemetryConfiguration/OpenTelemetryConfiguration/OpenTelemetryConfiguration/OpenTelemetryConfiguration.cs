using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryConfiguration.Models;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace OpenTelemetryConfiguration
{
    public static class OpenTelemetryConfiguration
    {
        public const string _otelCollectorEndpoint = "http://localhost:4317";

        public static void ConfigureSerilog(IConfiguration configuration, ServiceInfo serviceInfo)
        {
            var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? _otelCollectorEndpoint;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithOpenTelemetryTraceId()
                .Enrich.WithOpenTelemetrySpanId()
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceInfo.Name,
                        ["service.version"] = serviceInfo.Version,
                        ["service.environment"] = serviceInfo.Environment
                    };

                    options.IncludedData = IncludedData.MessageTemplateRenderingsAttribute
                        | IncludedData.MessageTemplateTextAttribute
                        | IncludedData.TraceIdField
                        | IncludedData.SpanIdField
                        | IncludedData.SourceContextAttribute
                        | IncludedData.MessageTemplateRenderingsAttribute
                        | IncludedData.SpecRequiredResourceAttributes;
                })
                .CreateLogger();
        }

        public static OpenTelemetryBuilder AddOpenTelemetry(
            this IServiceCollection services,
            IConfiguration configuration,
            ServiceInfo serviceInfo)
        {
            var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? _otelCollectorEndpoint;

            return services.AddOpenTelemetry()
                .ConfigureResource(builder =>
                {
                    builder.AddService
                    (
                        serviceName: serviceInfo.Name,
                        serviceVersion: serviceInfo.Version,
                        serviceInstanceId: serviceInfo.Name + "-" + serviceInfo.Environment,
                        autoGenerateServiceInstanceId: false);

                    builder.AddAttributes(
                    [
                        new KeyValuePair<string, object>("service.environment", serviceInfo.Environment)
                    ]);
                })
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSqlClientInstrumentation()
                    .AddNpgsql()
                    .AddOtlpExporter(o => 
                    {
                        o.Endpoint = new Uri(otlpEndpoint);
                        o.ExportProcessorType = ExportProcessorType.Batch;
                    }))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(otlpEndpoint);
                        o.ExportProcessorType = ExportProcessorType.Batch;
                    }));
        }
    }
}
