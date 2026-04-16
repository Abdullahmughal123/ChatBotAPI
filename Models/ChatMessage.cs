using System.ComponentModel.DataAnnotations;

namespace ChatbotApi.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string UserMessage { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string BotResponse { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
