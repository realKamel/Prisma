using FluentValidation;

namespace Prisma.Application.Features.Chats.Commands.CreateDirectConversation;

public sealed class CreateDirectConversationCommandValidator
    : AbstractValidator<CreateDirectConversationCommand>
{
    public CreateDirectConversationCommandValidator()
    {
        RuleFor(x => x.OtherUserId).NotEmpty();
    }
}