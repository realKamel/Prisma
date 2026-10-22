using FluentValidation;

namespace Prisma.Application.Features.Chats.Commands.MarkAsRead;

public sealed class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.LastMessageId).NotEmpty();
    }
}