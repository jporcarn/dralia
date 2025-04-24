using AutoMapper;
using System.Net;
using System.Text.Json;

namespace Docplanner.Api.Middlewares
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions and returns a JSON response with the error details.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        /// <remarks>By design the API uses Built-in standard exceptions in .NET, so I don't need to create a custom exception.
        /// This approach keeps the code clean and leverages built-in .NET exceptions effectively.
        /// </remarks>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string? json = null;

            if (exception is ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Invalid request: {Message}", argEx.Message);

                var errorDetails = new
                {
                    Title = "Bad Request",
                    Detail = argEx.Message
                };

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is AutoMapperMappingException mapEx)
            {
                _logger.LogWarning(mapEx, "AutoMapper mapping failed.");

                var errorDetails = new
                {
                    DestinationMember = mapEx.MemberMap?.DestinationName,
                    Detail = mapEx.InnerException?.Message ?? mapEx.Message,
                    MappingDestinationType = mapEx.Types?.DestinationType?.FullName,
                    MappingSourceType = mapEx.Types?.SourceType?.FullName,
                    Path = mapEx.MemberMap?.SourceMember?.DeclaringType?.FullName,
                    SourceMember = mapEx.MemberMap?.SourceMember?.Name,
                    Title = "Data Processing Error",
                };

                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is AutoMapperConfigurationException configEx)
            {
                _logger.LogWarning(configEx, "AutoMapper configuration is invalid.");

                var errorDetails = new
                {
                    Detail = configEx.Message,
                    Title = "Mapping Configuration Error",
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is KeyNotFoundException keyEx)
            {
                _logger.LogWarning(keyEx, "the server cannot find the requested resource");

                var errorDetails = new
                {
                    Title = "the server cannot find the requested resource",
                    Detail = keyEx.Message
                };

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is InvalidOperationException opEx)
            {
                _logger.LogWarning(opEx, "Invalid operation occurred during data processing.");

                var errorDetails = new
                {
                    Title = "Data Processing Error",
                    Detail = opEx.Message
                };

                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is TaskCanceledException || exception is OperationCanceledException)
            {
                _logger.LogWarning(exception, "A request to an external service timed out.");

                var errorDetails = new
                {
                    Title = "Request Timeout",
                    Detail = "The request to an external service timed out. Please try again later."
                };

                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is HttpRequestException httpEx)
            {
                _logger.LogWarning(httpEx, "An HTTP request to an external service failed.");

                var errorDetails = new
                {
                    Title = "External Service Error",
                    Detail = httpEx.Message
                };

                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable; // TODO: Decide whether to propagate the external API status code or not. E.g.: ((int?)httpEx.StatusCode) ?? StatusCodes.Status503ServiceUnavailable
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            if (exception is JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Failed to process JSON data.");

                var errorDetails = new
                {
                    Title = "Invalid JSON Format",
                    Detail = jsonEx.Message
                };

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                json = JsonSerializer.Serialize(errorDetails);
                return context.Response.WriteAsync(json);
            }

            // Other exceptions
            _logger.LogWarning(exception, "An unexpected error occurred.");
            var response = new
            {
                Title = "An unexpected error occurred.",
                Detail = exception.Message,
                Status = (int)HttpStatusCode.InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            json = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(json);
        }
    }
}