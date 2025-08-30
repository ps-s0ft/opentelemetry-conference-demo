using GameClientPublisher.Infrastructure.DataBaseContext;
using GameClientPublisher.Infrastructure.Extensions;
using GameClientPublisher.Services;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryConfiguration;
using OpenTelemetryConfiguration.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var test = builder.Configuration.GetConnectionString("GameDb");

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(test));


var serviceInfo = new ServiceInfo
{
    Name = "GameClientPublisher",
    Version = "1.3.0"
};

OpenTelemetryConfiguration.OpenTelemetryConfiguration.ConfigureSerilog(builder.Configuration, serviceInfo);
builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry(builder.Configuration, serviceInfo);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IManagementService, ManagementService>();

var app = builder.Build();

SetDBDataExtension.SetDbDefaultData(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();