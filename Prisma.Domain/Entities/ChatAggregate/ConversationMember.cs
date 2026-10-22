using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.Chat;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.ChatAggregate;

public sealed class ConversationMember : BaseEntity<Guid>
{
    private ConversationMember() { }

    internal ConversationMember(
        Guid conversationId,
        Guid userId,
        ConversationRole role,
        DateTimeOffset joinedAt
    )
    {
        Id = Guid.CreateVersion7();
        ConversationId = conversationId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }

    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public ConversationRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public Guid? LastReadMessageId { get; private set; }
    public DateTimeOffset? LastReadAt { get; private set; }

    /// <summary>
    /// Advances the read pointer. <paramref name="lastMessageSentAt" /> is stored as
    /// <see cref="LastReadAt" /> so unread counts compare message timestamps against the
    /// exact point in the conversation the member has seen.
    /// </summary>
    public Result MarkAsRead(
        Guid lastMessageId,
        DateTimeOffset lastMessageSentAt,
        DateTimeOffset readAt
    )
    {
        if (lastMessageId == Guid.Empty)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(lastMessageId),
                    ErrorMessage = "A valid last message id is required.",
                }
            );
        }

        if (readAt < lastMessageSentAt)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(readAt),
                    ErrorMessage = "A message cannot be read before it was sent.",
                }
            );
        }

        if (LastReadAt.HasValue && LastReadAt.Value >= lastMessageSentAt)
        {
            return Result.Success();
        }

        AdvanceReadPointer(lastMessageId, lastMessageSentAt);

        AddDomainEvent(new MessageReadDomainEvent(ConversationId, UserId, lastMessageId, readAt));

        return Result.Success();
    }

    /// <summary>
    /// Moves the read pointer without raising a domain event, used when the member sends
    /// a message and therefore has implicitly seen everything before it.
    /// </summary>
    internal void AdvanceReadPointer(Guid lastMessageId, DateTimeOffset lastMessageSentAt)
    {
        if (LastReadAt.HasValue && LastReadAt.Value >= lastMessageSentAt)
        {
            return;
        }

        LastReadMessageId = lastMessageId;
        LastReadAt = lastMessageSentAt;
    }

    public void PromoteToAdmin() => Role = ConversationRole.Admin;
}
