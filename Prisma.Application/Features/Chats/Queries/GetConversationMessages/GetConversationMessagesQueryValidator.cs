using FluentValidation;

namespace Prisma.Application.Features.Chats.Queries.GetConversationMessages;

public sealed class GetConversationMessagesQueryValidator
    : AbstractValidator<GetConversationMessagesQuery>
{
    public GetConversationMessagesQueryValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}