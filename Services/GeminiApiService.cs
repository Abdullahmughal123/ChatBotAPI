using ChatbotApi.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ChatbotApi.Services
{
    public class GeminiApiService : IGeminiApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiApiService> _logger;
        private readonly string _apiKey;

        public GeminiApiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
            
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        }

        public async Task<string> GenerateResponseAsync(string message)
        {
            const int maxRetries = 3;
            const int delayMs = 2500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = message }
                                }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                    });
                    
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(
                        $"v1/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                        content
                    );

                    var responseString = await response.Content.ReadAsStringAsync();
                    
                    _logger.LogInformation("Gemini API Status (Attempt {Attempt}): {StatusCode}", attempt, response.StatusCode);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || 
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (attempt < maxRetries)
                        {
                            _logger.LogWarning("Gemini API returned {StatusCode}. Retrying in {Delay}ms... (Attempt {Attempt}/{MaxRetries})", 
                                response.StatusCode, delayMs, attempt, maxRetries);
                            
                            await Task.Delay(delayMs);
                            continue;
                        }
                        else
                        {
                            _logger.LogWarning("Gemini API failed with {StatusCode} after {maxRetries} attempts. Using fallback response.", 
                                response.StatusCode, maxRetries);
                            return GetFallbackResponse(message);
                        }
                    }
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"Gemini API failed with status {response.StatusCode}: {responseString}");
                    }

                    var geminiResult = JsonSerializer.Deserialize<JsonElement>(responseString);

                    if (geminiResult.TryGetProperty("error", out var error))
                    {
                        var errorMessage = error.GetProperty("message").GetString();
                        throw new InvalidOperationException($"Gemini API error: {errorMessage}");
                    }

                    if (geminiResult.TryGetProperty("candidates", out var candidates) && 
                        candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentElement) &&
                            contentElement.TryGetProperty("parts", out var parts) &&
                            parts.GetArrayLength() > 0)
                        {
                            var part = parts[0];
                            if (part.TryGetProperty("text", out var textElement))
                            {
                                var responseText = textElement.GetString();
                                _logger.LogInformation("Successfully extracted Gemini response on attempt {Attempt}", attempt);
                                return responseText ?? string.Empty;
                            }
                        }
                    }

                    throw new InvalidOperationException("Failed to parse Gemini response structure");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception calling Gemini API (Attempt {Attempt})", attempt);
                    
                    if (attempt == maxRetries)
                    {
                        throw;
                    }
                    
                    if (ex is InvalidOperationException)
                    {
                        throw;
                    }
                    
                    await Task.Delay(delayMs);
                }
            }

            throw new InvalidOperationException("Maximum retry attempts exceeded for Gemini API call");
        }

        private string GetFallbackResponse(string userMessage)
        {
            _logger.LogInformation("Using fallback response for message: {Message}", userMessage);
            
            var fallbackResponses = new[]
            {
                "I'm currently experiencing some technical difficulties with my AI service. Please try again in a few moments.",
                "My AI service is temporarily unavailable. I'm still here to help - could you please rephrase your message or try again?",
                "I'm having trouble connecting to my AI backend right now. Please try again shortly, and I'll be happy to assist you.",
                "Due to high demand, my AI service is temporarily unavailable. Please try again in a moment.",
                "I'm experiencing a temporary service issue. Please try your request again in a few moments."
            };

            var random = new Random();
            var response = fallbackResponses[random.Next(fallbackResponses.Length)];
            
            return response;
        }
    }
}
