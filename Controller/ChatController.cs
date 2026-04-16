using ChatbotApi.DTOs;
using ChatbotApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ChatResponseDto>>> SendMessage([FromBody] ChatRequestDto request)
        {
            return Ok(await _chatService.ProcessMessageAsync(request));
        }

        [HttpGet("history")]
        public async Task<ActionResult<PagedResponse<ChatMessageDto>>> GetChatHistory([FromQuery] GetChatHistoryQuery query)
        {
            return Ok(await _chatService.GetChatHistoryAsync(query));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ChatMessageDto>>> GetChatMessage(int id)
        {
            return Ok(await _chatService.GetChatMessageAsync(id));
        }
    }
}
