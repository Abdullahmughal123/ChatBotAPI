using ChatbotApi.DTOs;
using FluentValidation;

namespace ChatbotApi.Validators
{
    public class ChatRequestValidator : AbstractValidator<ChatRequestDto>
    {
        public ChatRequestValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required")
                .MaximumLength(1000)
                .WithMessage("Message cannot exceed 1000 characters")
                .MinimumLength(1)
                .WithMessage("Message cannot be empty");

            RuleFor(x => x.Message)
                .Must(message => !string.IsNullOrWhiteSpace(message))
                .WithMessage("Message cannot be whitespace only");
        }
    }

    public class GetChatHistoryQueryValidator : AbstractValidator<GetChatHistoryQuery>
    {
        public GetChatHistoryQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100");
        }
    }
}
