using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Queries.GetConversationMessages;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Tests.Features.Chats;

public class GetConversationMessagesQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();
    private readonly IConversationRepository _conversationRepository =
        Substitute.For<IConversationRepository>();
    private readonly GetConversationMessagesQueryHandler _sut;

    public GetConversationMessagesQueryHandlerTests()
    {
        _sut = new GetConversationMessagesQueryHandler(
            _currentUserService,
            _conversationRepository
        );
    }

    [Fact]
    public async Task Handle_WhenUserIsNotMember_ReturnsForbidden()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _conversationRepository
            .IsMemberAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(false);

        var result = await _sut.Handle(
            new GetConversationMessagesQuery(Guid.NewGuid(), null, 20),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WithMoreMessagesThanPageSize_ReturnsCursorPage()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(userId, otherUserId).Value;
        var baseTime = DateTimeOffset.UtcNow.AddMinutes(-10);

        var first = conversation.AddMessage(userId, "first", baseTime).Value;
        var second = conversation.AddMessage(otherUserId, "second", baseTime.AddMinutes(1)).Value;
        var third = conversation.AddMessage(userId, "third", baseTime.AddMinutes(2)).Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .IsMemberAsync(conversation.Id, userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _conversationRepository
            .GetMessagesPageAsync(
                conversation.Id,
                null,
                3,
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<Message> { third, second, first });

        var result = await _sut.Handle(
            new GetConversationMessagesQuery(conversation.Id, null, 2),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Select(message => message.Id).Should().ContainInOrder(third.Id, second.Id);
        result.Value.HasMore.Should().BeTrue();
        result.Value.NextCursor.Should().Be(second.SentAt);
    }

    [Fact]
    public async Task Handle_WhenNoMoreMessages_ReturnsNullCursor()
    {
        var userId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(userId, Guid.NewGuid()).Value;
        var message = conversation.AddMessage(userId, "only").Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .IsMemberAsync(conversation.Id, userId, Arg.Any<CancellationToken>())
            .Returns(true);
        _conversationRepository
            .GetMessagesPageAsync(
                conversation.Id,
                Arg.Any<DateTimeOffset?>(),
                21,
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<Message> { message });

        var result = await _sut.Handle(
            new GetConversationMessagesQuery(conversation.Id, null, 20),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.HasMore.Should().BeFalse();
        result.Value.NextCursor.Should().BeNull();
    }
}