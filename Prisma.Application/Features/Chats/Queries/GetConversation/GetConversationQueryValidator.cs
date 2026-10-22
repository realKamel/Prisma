using FluentValidation;

namespace Prisma.Application.Features.Chats.Queries.GetConversation;

public sealed class GetConversationQueryValidator : AbstractValidator<GetConversationQuery>
{
    public GetConversationQueryValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
    }
}