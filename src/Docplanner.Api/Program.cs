using Docplanner.Api.Handlers;
using Docplanner.Api.Middlewares;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Domain.Models.Configuration;
using Docplanner.Infrastructure.SlotService.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// IOptions<> pattern provides a way to access configuration settings in a strongly typed manner.
// Bind the AvailabilityApi section to AvailabilityApiOptions
builder.Services.Configure<AvailabilityApiOptions>(builder.Configuration.GetSection("AvailabilityApi"));

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

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200", // Allow requests from Angular app
            "https://white-pebble-0a6d1d903.6.azurestaticapps.net" // Allow requests from Azure Static Web App
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
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

builder.Services.AddHttpClient<IAvailabilityRepository, AvailabilityApiRepository>((sp, client) =>
{
    // Retrieve the AvailabilityApiOptions from the service provider
    var configuration = sp.GetRequiredService<IOptions<AvailabilityApiOptions>>().Value;

    if (!String.IsNullOrWhiteSpace(configuration.BaseUrl))
    {
        client.BaseAddress = new Uri(configuration.BaseUrl);
    }
})
    .AddHttpMessageHandler((sp) =>
    {
        var configuration = sp.GetRequiredService<IOptions<AvailabilityApiOptions>>().Value;

        return new BasicAuthHandler(configuration.Credentials.Username, configuration.Credentials.Password);
    });

// Use AddScoped to align their lifecycle with the HTTP request.
// This ensures efficient resource usage and avoids potential lifecycle mismatches.
builder.Services.AddScoped<IGetAvailableSlotsHandler, GetAvailableSlotsHandler>();
builder.Services.AddScoped<IBookSlotHandler, BookSlotHandler>();

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

app.UseCors("AllowSpecificOrigins");

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