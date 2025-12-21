using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Create custom meter for application metrics
var meter = new Meter("SimpleApi.CustomMetrics", "1.0.0");

// Create custom metric instruments
var requestCounter = meter.CreateCounter<long>("simple.api.requests.count", description: "Total number of API requests");
var dataRequestCounter = meter.CreateCounter<long>("simple.api.data.requests", description: "Number of requests to /api/data");
var processingTimeHistogram = meter.CreateHistogram<double>("simple.api.processing.duration", unit: "ms", description: "API request processing time");

// Configure OpenTelemetry Metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "SimpleApi", serviceVersion: "1.0.0"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("SimpleApi.CustomMetrics") // Register custom meter
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
    var startTime = DateTime.UtcNow;
    
    logger.LogInformation("Processing request to /api/data endpoint");
    
    // Record custom metrics
    requestCounter.Add(1);
    dataRequestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/api/data"));
    
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
    
    // Record processing time
    var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
    processingTimeHistogram.Record(processingTime, new KeyValuePair<string, object?>("endpoint", "/api/data"));
    
    return Results.Ok(data);
})
.WithName("GetData")
.WithOpenApi();

app.Run();
