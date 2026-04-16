using ChatbotApi.DTOs;
using ChatbotApi.Exceptions;
using System.Text.Json;

namespace ChatbotApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ArgumentException => CreateErrorResponse("Invalid argument provided", StatusCodes.Status400BadRequest, new List<string> { exception.Message }),
                NotFoundException => CreateErrorResponse("Resource not found", StatusCodes.Status404NotFound, new List<string> { exception.Message }),
                ValidationException validationEx => CreateErrorResponse("Validation failed", StatusCodes.Status400BadRequest, validationEx.Errors),
                InvalidOperationException => CreateErrorResponse("Service temporarily unavailable", StatusCodes.Status503ServiceUnavailable, new List<string> { exception.Message }),
                _ => CreateErrorResponse("An internal error occurred", StatusCodes.Status500InternalServerError, new List<string> { "An unexpected error occurred. Please try again later." })
            };

            context.Response.StatusCode = response.StatusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response.ApiResponse, jsonOptions));
        }

        private static (int StatusCode, ApiResponse ApiResponse) CreateErrorResponse(string message, int statusCode, List<string> errors)
        {
            return (statusCode, new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
