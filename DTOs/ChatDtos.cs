using System.ComponentModel.DataAnnotations;

namespace ChatbotApi.DTOs
{
    public class ChatRequestDto
    {
        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 1000 characters")]
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class GetChatHistoryQuery
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;
    }

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
