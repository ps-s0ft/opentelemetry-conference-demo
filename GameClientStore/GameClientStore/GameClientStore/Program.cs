using GameClientStore.Services;
using OpenTelemetryConfiguration;
using OpenTelemetryConfiguration.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var serviceInfo = new ServiceInfo
{
    Name = "GameClientStore",
    Version = "1.2.0"
};

OpenTelemetryConfiguration.OpenTelemetryConfiguration.ConfigureSerilog(builder.Configuration, serviceInfo: serviceInfo);
builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry(builder.Configuration, serviceInfo: serviceInfo);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserGamesService, UserGamesService>();

builder.Services.AddHttpClient("GameClientPublisher", c =>
{
    c.BaseAddress = new Uri("https://localhost:5002");
});

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
