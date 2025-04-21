using AutoMapper;
using Docplanner.Api.Handlers;
using Docplanner.Api.Middlewares;
using Docplanner.Application.Interfaces.Repositories;
using Docplanner.Application.UseCases.Availability;
using Docplanner.Infrastructure.SlotService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Access Configuration
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();

public partial class Program
{ }