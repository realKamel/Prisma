using FluentValidation;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Application.Features.Chats.Commands.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(Conversation.MaxMessageLength);
    }
}