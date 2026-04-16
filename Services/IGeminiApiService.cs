namespace ChatbotApi.Services
{
    public interface IGeminiApiService
    {
        Task<string> GenerateResponseAsync(string message);
    }
}
