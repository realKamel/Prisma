using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Queries.GetConversation;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Tests.Features.Chats;

public class GetConversationQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();
    private readonly IConversationRepository _conversationRepository =
        Substitute.For<IConversationRepository>();

    [Fact]
    public async Task Handle_WhenConversationIsMissing_ReturnsNotFound()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _conversationRepository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var sut = new GetConversationQueryHandler(
            _currentUserService,
            _conversationRepository
        );

        var result = await sut.Handle(
            new GetConversationQuery(Guid.NewGuid()),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotMember_ReturnsForbidden()
    {
        var conversation = Conversation.CreateDirect(Guid.NewGuid(), Guid.NewGuid()).Value;

        _currentUserService.UserId.Returns(Guid.NewGuid());
        _conversationRepository
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var sut = new GetConversationQueryHandler(
            _currentUserService,
            _conversationRepository
        );

        var result = await sut.Handle(
            new GetConversationQuery(conversation.Id),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenUserIsMember_ReturnsMappedConversation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(userId, otherUserId).Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var sut = new GetConversationQueryHandler(
            _currentUserService,
            _conversationRepository
        );

        var result = await sut.Handle(
            new GetConversationQuery(conversation.Id),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(conversation.Id);
        result.Value.Type.Should().Be(ConversationType.Direct);
        result.Value.Members.Should().HaveCount(2);
        result.Value.Members.Select(member => member.UserId)
            .Should()
            .BeEquivalentTo([userId, otherUserId]);
    }
}