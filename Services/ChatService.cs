using ChatbotApi.DTOs;
using ChatbotApi.Data;
using ChatbotApi.Models;
using ChatbotApi.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ChatbotApi.Services
{
    public class ChatService : IChatService
    {
        private readonly IGeminiApiService _geminiApiService;
        private readonly AppDbContext _context;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IGeminiApiService geminiApiService, AppDbContext context, ILogger<ChatService> logger)
        {
            _geminiApiService = geminiApiService ?? throw new ArgumentNullException(nameof(geminiApiService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<ChatResponseDto>> ProcessMessageAsync(ChatRequestDto request)
        {
            _logger.LogInformation("Processing chat message");

            if (request == null)
                throw new ArgumentException("Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message cannot be null or empty");

            try
            {
                var aiResponse = await _geminiApiService.GenerateResponseAsync(request.Message);
                
                await SaveChatMessageAsync(request.Message, aiResponse);
                
                _logger.LogInformation("Chat message processed successfully");
                
                return new ApiResponse<ChatResponseDto>
                {
                    Success = true,
                    Message = "Message processed successfully",
                    Data = new ChatResponseDto
                    {
                        Response = aiResponse,
                        Timestamp = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message: {Message}", request.Message);
                throw new InvalidOperationException("Failed to process chat message", ex);
            }
        }

        public async Task<PagedResponse<ChatMessageDto>> GetChatHistoryAsync(GetChatHistoryQuery query)
        {
            _logger.LogInformation("Retrieving chat history - Page: {Page}, PageSize: {PageSize}", query.Page, query.PageSize);

            if (query == null)
                throw new ArgumentException("Query cannot be null");

            try
            {
                var totalCount = await _context.ChatMessages.CountAsync();
                
                var messages = await _context.ChatMessages
                    .OrderByDescending(cm => cm.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(cm => new ChatMessageDto
                    {
                        Id = cm.Id,
                        UserMessage = cm.UserMessage,
                        BotResponse = cm.BotResponse,
                        CreatedAt = cm.CreatedAt
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

                _logger.LogInformation("Retrieved {Count} chat messages out of {TotalCount}", messages.Count, totalCount);

                return new PagedResponse<ChatMessageDto>
                {
                    Success = true,
                    Message = "Chat history retrieved successfully",
                    Data = messages,
                    Pagination = new PaginationInfo
                    {
                        Page = query.Page,
                        PageSize = query.PageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = query.Page < totalPages,
                        HasPreviousPage = query.Page > 1
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve chat history");
                throw new InvalidOperationException("Failed to retrieve chat history", ex);
            }
        }

        public async Task<ApiResponse<ChatMessageDto>> GetChatMessageAsync(int id)
        {
            _logger.LogInformation("Retrieving chat message with ID: {Id}", id);

            if (id <= 0)
                throw new ArgumentException("Invalid chat message ID");

            try
            {
                var message = await _context.ChatMessages
                    .Where(cm => cm.Id == id)
                    .Select(cm => new ChatMessageDto
                    {
                        Id = cm.Id,
                        UserMessage = cm.UserMessage,
                        BotResponse = cm.BotResponse,
                        CreatedAt = cm.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (message == null)
                {
                    _logger.LogWarning("Chat message with ID {Id} not found", id);
                    throw new NotFoundException($"Chat message with ID {id} not found");
                }

                _logger.LogInformation("Chat message with ID {Id} retrieved successfully", id);
                
                return new ApiResponse<ChatMessageDto>
                {
                    Success = true,
                    Message = "Chat message retrieved successfully",
                    Data = message
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve chat message with ID: {Id}", id);
                throw new InvalidOperationException("Failed to retrieve chat message", ex);
            }
        }

        private async Task SaveChatMessageAsync(string userMessage, string botResponse)
        {
            try
            {
                var chatMessage = new ChatMessage
                {
                    UserMessage = userMessage,
                    BotResponse = botResponse,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Chat message saved successfully with ID: {Id}", chatMessage.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save chat message to database");
                throw new InvalidOperationException("Failed to save chat message", ex);
            }
        }
    }
}
