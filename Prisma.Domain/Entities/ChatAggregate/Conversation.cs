using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.DomainEvents.Chat;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.ChatAggregate;

/// <summary>
/// Unified conversation aggregate for both direct (1-on-1) and group chats.
/// </summary>
public sealed class Conversation : BaseEntity<Guid>
{
    public const int MaxTitleLength = 200;
    public const int MaxMessageLength = 4000;

    private readonly List<ConversationMember> _members = [];
    private readonly List<Message> _messages = [];

    private Conversation() { }

    private Conversation(ConversationType type, string? title)
    {
        Type = type;
        Title = title;
    }

    public ConversationType Type { get; private set; }
    public string? Title { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }

    public IReadOnlyCollection<ConversationMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    public bool HasMember(Guid userId) => _members.Exists(m => m.UserId == userId);

    public ConversationMember? GetMember(Guid userId) => _members.Find(m => m.UserId == userId);

    public static Result<Conversation> CreateDirect(Guid initiatedByUserId, Guid otherUserId)
    {
        var validationErrors = new List<ValidationError>();

        if (initiatedByUserId == Guid.Empty)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(initiatedByUserId),
                    ErrorMessage = "The initiating user id is required.",
                }
            );
        }

        if (otherUserId == Guid.Empty)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(otherUserId),
                    ErrorMessage = "The other participant's user id is required.",
                }
            );
        }

        if (initiatedByUserId == otherUserId)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(otherUserId),
                    ErrorMessage = "A direct conversation requires two different participants.",
                }
            );
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors);
        }

        var now = DateTimeOffset.UtcNow;
        var conversation = new Conversation(ConversationType.Direct, null);

        conversation.AppendMember(initiatedByUserId, ConversationRole.Member, now);
        conversation.AppendMember(otherUserId, ConversationRole.Member, now);

        return Result.Success(conversation);
    }

    public static Result<Conversation> CreateGroup(
        Guid creatorUserId,
        string title,
        IEnumerable<Guid> memberIds
    )
    {
        ArgumentNullException.ThrowIfNull(memberIds);

        var validationErrors = new List<ValidationError>();

        if (creatorUserId == Guid.Empty)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(creatorUserId),
                    ErrorMessage = "The creator's user id is required.",
                }
            );
        }

        var normalizedTitle = title?.Trim() ?? string.Empty;

        if (normalizedTitle.Length == 0)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(title),
                    ErrorMessage = "A group conversation requires a title.",
                }
            );
        }

        if (normalizedTitle.Length > MaxTitleLength)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(title),
                    ErrorMessage = $"A group title cannot exceed {MaxTitleLength} characters.",
                }
            );
        }

        var otherMemberIds = memberIds
            .Where(id => id != Guid.Empty && id != creatorUserId)
            .Distinct()
            .ToList();

        if (otherMemberIds.Count == 0)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(memberIds),
                    ErrorMessage = "A group conversation requires at least one other member.",
                }
            );
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors);
        }

        var now = DateTimeOffset.UtcNow;
        var conversation = new Conversation(ConversationType.Group, normalizedTitle);

        conversation.AppendMember(creatorUserId, ConversationRole.Admin, now);

        foreach (var memberId in otherMemberIds)
        {
            conversation.AppendMember(memberId, ConversationRole.Member, now);
        }

        return Result.Success(conversation);
    }

    public Result<Message> AddMessage(Guid senderId, string content, DateTimeOffset? sentAt = null)
    {
        var validationErrors = new List<ValidationError>();

        if (!HasMember(senderId))
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(senderId),
                    ErrorMessage = "Only conversation members can send messages.",
                }
            );
        }

        var normalizedContent = content?.Trim() ?? string.Empty;

        if (normalizedContent.Length == 0)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(content),
                    ErrorMessage = "Message content is required.",
                }
            );
        }

        if (normalizedContent.Length > MaxMessageLength)
        {
            validationErrors.Add(
                new ValidationError
                {
                    Identifier = nameof(content),
                    ErrorMessage = $"Message content cannot exceed {MaxMessageLength} characters.",
                }
            );
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors);
        }

        var timestamp = sentAt ?? DateTimeOffset.UtcNow;
        var message = Message.Create(Id, senderId, normalizedContent, timestamp);

        _messages.Add(message);
        LastMessageAt = timestamp;

        // Sending a message implies the sender has seen the conversation up to that point.
        GetMember(senderId)!.AdvanceReadPointer(message.Id, timestamp);

        AddDomainEvent(
            new MessageSentDomainEvent(Id, message.Id, senderId, normalizedContent, timestamp)
        );

        return Result.Success(message);
    }

    public Result MarkAsRead(
        Guid userId,
        Guid lastMessageId,
        DateTimeOffset lastMessageSentAt,
        DateTimeOffset readAt
    )
    {
        var member = GetMember(userId);

        if (member is null)
        {
            return Result.Invalid(
                new ValidationError
                {
                    Identifier = nameof(userId),
                    ErrorMessage = "Only conversation members can update read receipts.",
                }
            );
        }

        return member.MarkAsRead(lastMessageId, lastMessageSentAt, readAt);
    }

    private void AppendMember(Guid userId, ConversationRole role, DateTimeOffset joinedAt)
    {
        _members.Add(new ConversationMember(Id, userId, role, joinedAt));
    }
}
