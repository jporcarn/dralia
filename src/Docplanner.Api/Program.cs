using Docplanner.Api.Handlers;
using Docplanner.Api.Middlewares;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Infrastructure.SlotService.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Access Configuration
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Docplanner API",
        Version = "v1.6",
        Description = "API for managing availability and slots in Docplanner.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Josep Porcar Nadal",
            Email = "jjppnn@hotmail.com",
            Url = new Uri("https://www.linkedin.com/in/josep-porcar-nadal-08695550/")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Register custom error handling middleware
builder.Services.AddTransient<ErrorHandlingMiddleware>();

// Register AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

string baseUrlString = configuration.GetValue<string>("AvailabilityApi:BaseUrl") ?? "http://localhost/api";

builder.Services.AddHttpClient<IAvailabilityRepository, AvailabilityApiRepository>(client =>
{
    client.BaseAddress = new Uri(baseUrlString);
})
    .AddHttpMessageHandler(() =>
    {
        var config = builder.Configuration.GetSection("AvailabilityApi:Credentials");

        string username = (Environment.GetEnvironmentVariable("AVAILABILITYAPI__CREDENTIALS__USERNAME")
                          ?? config["Username"]) ?? string.Empty;

        string password = (Environment.GetEnvironmentVariable("AVAILABILITYAPI__CREDENTIALS__PASSWORD")
                          ?? config["Password"]) ?? string.Empty;

        return new BasicAuthHandler(config["Username"] ?? string.Empty, config["Password"] ?? string.Empty);
    });

// Use AddScoped to align their lifecycle with the HTTP request.
// This ensures efficient resource usage and avoids potential lifecycle mismatches.
builder.Services.AddScoped<IGetAvailableSlotsHandler, GetAvailableSlotsHandler>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("Docplanner API", () =>
    {
        return HealthCheckResult.Healthy("Docplanner API is reachable.");
    })

    // This does not ensure that the HttpClient is resolved according to the setup in AddHttpClient.
    // .AddCheck<AvailabilityApiRepository>("Availability API")

    // This uses a factory method to resolve AvailabilityApiRepository from the DI container.
    // Required to resolve correctly the HttpClient including the BasicAuthHandler
    .AddAsyncCheck("Availability API", async (cancellationToken) =>
    {
        using var scope = builder.Services.BuildServiceProvider().CreateScope(); // TODO: Use a better way to resolve the service
        var repository = scope.ServiceProvider.GetRequiredService<IAvailabilityRepository>() as AvailabilityApiRepository;

        if (repository == null)
        {
            return HealthCheckResult.Unhealthy("AvailabilityApiRepository could not be resolved.");
        }

        return await repository.CheckHealthAsync(new HealthCheckContext(), cancellationToken);
    })
    .AddCheck("Ping", () => HealthCheckResult.Healthy("Pong"));

var app = builder.Build();

// Global error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/healthcheck", new HealthCheckOptions
{
    Predicate = (check) => !check.Name.Contains("ping", StringComparison.OrdinalIgnoreCase), // Exclude the "ping" health check
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                data = entry.Value.Data
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/ping", new HealthCheckOptions
{
    Predicate = (check) => check.Name == "ping", // Include only the "ping" health check
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsync("Pong");
    }
});

app.Run();

// Required to make the Program available to the test projects
public partial class Program
{ }