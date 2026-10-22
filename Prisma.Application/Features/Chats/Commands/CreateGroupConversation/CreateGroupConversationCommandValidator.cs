using FluentValidation;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Application.Features.Chats.Commands.CreateGroupConversation;

public sealed class CreateGroupConversationCommandValidator
    : AbstractValidator<CreateGroupConversationCommand>
{
    public CreateGroupConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(Conversation.MaxTitleLength);

        RuleFor(x => x.MemberIds).NotEmpty();
        RuleForEach(x => x.MemberIds).NotEmpty();
    }
}