using ChatbotApi.DTOs;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace ChatbotApi.Middleware
{
    public class ModelValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ModelValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }
}
