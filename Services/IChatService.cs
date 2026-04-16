using ChatbotApi.DTOs;

namespace ChatbotApi.Services
{
    public interface IChatService
    {
        Task<ApiResponse<ChatResponseDto>> ProcessMessageAsync(ChatRequestDto request);
        Task<PagedResponse<ChatMessageDto>> GetChatHistoryAsync(GetChatHistoryQuery query);
        Task<ApiResponse<ChatMessageDto>> GetChatMessageAsync(int id);
    }
}
