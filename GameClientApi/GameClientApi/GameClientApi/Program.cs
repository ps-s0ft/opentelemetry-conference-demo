using GameClientApi.Models;
using GameClientApi.Services;
using OpenTelemetryConfiguration;
using OpenTelemetryConfiguration.Models;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

var serviceInfo = new ServiceInfo
{
    Name = "GameClientApi",
    Version = "1.0.0"
};

OpenTelemetryConfiguration.OpenTelemetryConfiguration.ConfigureSerilog(
    builder.Configuration,
    serviceInfo: serviceInfo
);
builder.Host.UseSerilog();

var otelBuilder = builder.Services.AddOpenTelemetry(builder.Configuration, serviceInfo);
otelBuilder.WithTracing(t => t.AddSource(TelemetryName.CustomActivityName));
otelBuilder.WithMetrics(m => m.AddMeter(TelemetryName.CustomMeterName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("GameClientStore", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001");
});

builder.Services.AddSingleton(new ActivitySource(TelemetryName.CustomActivityName));
builder.Services.AddSingleton(new Meter(TelemetryName.CustomMeterName, "0.1.0"));

builder.Services.AddScoped<IUserGamesProxyService, UserGamesProxyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();