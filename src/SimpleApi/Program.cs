using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure OpenTelemetry Metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "SimpleApi", serviceVersion: "1.0.0"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(otlpOptions =>
        {
            // Grafana Alloy default OTLP endpoint
            otlpOptions.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
        }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
.WithName("HealthCheck")
.WithOpenApi();

// Simple GET endpoint that returns JSON data
app.MapGet("/api/data", (ILogger<Program> logger) =>
{
    logger.LogInformation("Processing request to /api/data endpoint");
    
    var data = new
    {
        Message = "Hello from Simple API!",
        Timestamp = DateTime.UtcNow,
        Version = "1.0.0",
        Items = new[]
        {
            new { Id = 1, Name = "Item One", Description = "First item" },
            new { Id = 2, Name = "Item Two", Description = "Second item" },
            new { Id = 3, Name = "Item Three", Description = "Third item" }
        }
    };
    return Results.Ok(data);
})
.WithName("GetData")
.WithOpenApi();

app.Run();
