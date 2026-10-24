using FluentAssertions;
using Prisma.Domain.DomainEvents.Chat;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Application.Tests.Features.Chats;

public class ConversationTests
{
    private static readonly Guid User1 = Guid.NewGuid();
    private static readonly Guid User2 = Guid.NewGuid();

    [Fact]
    public void CreateDirect_WithDistinctUsers_ReturnsDirectConversation()
    {
        var result = Conversation.CreateDirect(User1, User2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ConversationType.Direct);
        result.Value.Members.Should().HaveCount(2);
        result.Value.Members.Should().OnlyContain(member =>
            member.Role == ConversationRole.Member
        );
        result.Value.HasMember(User1).Should().BeTrue();
        result.Value.HasMember(User2).Should().BeTrue();
    }

    [Fact]
    public void CreateDirect_WithSameUser_ReturnsInvalid()
    {
        var result = Conversation.CreateDirect(User1, User1);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(error =>
            error.Identifier == "otherUserId"
        );
    }

    [Fact]
    public void CreateGroup_WithTitleAndMembers_CreatesAdminCreatorAndMembers()
    {
        var result = Conversation.CreateGroup(
            User1,
            "  Study group  ",
            [User2, Guid.NewGuid(), User2]
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ConversationType.Group);
        result.Value.Title.Should().Be("Study group");
        result.Value.Members.Should().HaveCount(3);
        result.Value.GetMember(User1)!.Role.Should().Be(ConversationRole.Admin);
        result.Value.Members.Count(member => member.Role == ConversationRole.Member)
            .Should()
            .Be(2);
    }

    [Fact]
    public void CreateGroup_WithoutTitle_ReturnsInvalid()
    {
        var result = Conversation.CreateGroup(User1, "   ", [User2]);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(error => error.Identifier == "title");
    }

    [Fact]
    public void AddMessage_ByMember_AddsMessageAndRaisesDomainEvent()
    {
        var conversation = Conversation.CreateDirect(User1, User2).Value;
        var sentAt = DateTimeOffset.UtcNow;

        var result = conversation.AddMessage(User1, "  hello there  ", sentAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("hello there");
        result.Value.SenderId.Should().Be(User1);
        result.Value.ConversationId.Should().Be(conversation.Id);
        conversation.Messages.Should().ContainSingle();
        conversation.LastMessageAt.Should().Be(sentAt);
        conversation.GetMember(User1)!.LastReadMessageId.Should().Be(result.Value.Id);

        var domainEvent = conversation.DomainEvents.Should().ContainSingle().Subject;
        domainEvent.Should().BeOfType<MessageSentDomainEvent>();

        var messageSent = (MessageSentDomainEvent)domainEvent;
        messageSent.ConversationId.Should().Be(conversation.Id);
        messageSent.MessageId.Should().Be(result.Value.Id);
        messageSent.SenderId.Should().Be(User1);
        messageSent.Content.Should().Be("hello there");
        messageSent.SentAt.Should().Be(sentAt);
    }

    [Fact]
    public void AddMessage_ByNonMember_ReturnsInvalidAndDoesNotRaiseEvent()
    {
        var conversation = Conversation.CreateDirect(User1, User2).Value;

        var result = conversation.AddMessage(Guid.NewGuid(), "hello");

        result.IsSuccess.Should().BeFalse();
        conversation.Messages.Should().BeEmpty();
        conversation.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddMessage_WithBlankContent_ReturnsInvalid()
    {
        var conversation = Conversation.CreateDirect(User1, User2).Value;

        var result = conversation.AddMessage(User1, "   ");

        result.IsSuccess.Should().BeFalse();
        conversation.Messages.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsRead_ByMember_AdvancesPointerAndRaisesDomainEvent()
    {
        var conversation = Conversation.CreateDirect(User1, User2).Value;
        var message = conversation.AddMessage(User1, "hello").Value;
        conversation.ClearDomainEvents();

        var readAt = DateTimeOffset.UtcNow;
        var result = conversation.MarkAsRead(User2, message.Id, message.SentAt, readAt);

        result.IsSuccess.Should().BeTrue();

        var member = conversation.GetMember(User2)!;
        member.LastReadMessageId.Should().Be(message.Id);
        member.LastReadAt.Should().Be(message.SentAt);

        var domainEvent = member.DomainEvents.Should().ContainSingle().Subject;
        domainEvent.Should().BeOfType<MessageReadDomainEvent>();

        var messageRead = (MessageReadDomainEvent)domainEvent;
        messageRead.ConversationId.Should().Be(conversation.Id);
        messageRead.UserId.Should().Be(User2);
        messageRead.LastReadMessageId.Should().Be(message.Id);
        messageRead.ReadAt.Should().Be(readAt);
    }

    [Fact]
    public void MarkAsRead_WithOlderPointer_IsNoOp()
    {
        var conversation = Conversation.CreateDirect(User1, User2).Value;
        var first = conversation.AddMessage(User1, "first").Value;
        conversation.AddMessage(User1, "second");

        var readAt = DateTimeOffset.UtcNow;
        conversation.MarkAsRead(User2, first.Id, first.SentAt, readAt);
        var member = conversation.GetMember(User2)!;
        var eventsAfterFirstRead = member.DomainEvents.Count;

        var result = conversation.MarkAsRead(User2, first.Id, first.SentAt, readAt.AddSeconds(1));

        result.IsSuccess.Should().BeTrue();
        member.DomainEvents.Should().HaveCount(eventsAfterFirstRead);
    }

    [Fact]
    public void MarkAsRead_ByNonMember_ReturnsInvalid()
    {
        var conversation = Conversation.CreateDirect(User1, User2).Value;
        var message = conversation.AddMessage(User1, "hello").Value;

        var result = conversation.MarkAsRead(
            Guid.NewGuid(),
            message.Id,
            message.SentAt,
            DateTimeOffset.UtcNow
        );

        result.IsSuccess.Should().BeFalse();
    }
}