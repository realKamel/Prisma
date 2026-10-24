using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Chats.Commands.CreateDirectConversation;
using Prisma.Application.Features.Chats.Commands.CreateGroupConversation;
using Prisma.Application.Features.Chats.Commands.MarkAsRead;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Tests.Features.Chats;

public class ConversationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();
    private readonly IConversationRepository _conversationRepository =
        Substitute.For<IConversationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateDirect_WhenConversationExists_ReturnsExistingIdWithoutCreatingAnother()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var existing = Conversation.CreateDirect(userId, otherUserId).Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetDirectConversationAsync(userId, otherUserId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var sut = new CreateDirectConversationCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );

        var result = await sut.Handle(
            new CreateDirectConversationCommand(otherUserId),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing.Id);

        await _conversationRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDirect_WhenMissing_CreatesAndPersistsDirectConversation()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetDirectConversationAsync(userId, otherUserId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var sut = new CreateDirectConversationCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );

        var result = await sut.Handle(
            new CreateDirectConversationCommand(otherUserId),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        await _conversationRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Conversation>(conversation =>
                    conversation.Id == result.Value
                    && conversation.Type == ConversationType.Direct
                    && conversation.HasMember(userId)
                    && conversation.HasMember(otherUserId)
                ),
                Arg.Any<CancellationToken>()
            );

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDirect_WhenUnauthenticated_ReturnsUnauthorized()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        var sut = new CreateDirectConversationCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );

        var result = await sut.Handle(
            new CreateDirectConversationCommand(Guid.NewGuid()),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task CreateGroup_CreatesConversationWithCreatorAsAdmin()
    {
        var userId = Guid.NewGuid();
        var memberIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _currentUserService.UserId.Returns(userId);

        var sut = new CreateGroupConversationCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );

        var result = await sut.Handle(
            new CreateGroupConversationCommand("Study group", memberIds),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        await _conversationRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Conversation>(conversation =>
                    conversation.Type == ConversationType.Group
                    && conversation.Title == "Study group"
                    && conversation.Members.Count == 3
                    && conversation.GetMember(userId)!.Role == ConversationRole.Admin
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task MarkAsRead_WhenMessageIsMissing_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(userId, Guid.NewGuid()).Value;

        _currentUserService.UserId.Returns(userId);
        _conversationRepository
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _conversationRepository
            .GetMessageAsync(
                conversation.Id,
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((Message?)null);

        var sut = new MarkAsReadCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );

        var result = await sut.Handle(
            new MarkAsReadCommand(conversation.Id, Guid.NewGuid()),
            CancellationToken.None
        );

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task MarkAsRead_WhenMemberReadsMessage_AdvancesPointerAndRaisesEvent()
    {
        var senderId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(senderId, readerId).Value;
        var message = conversation.AddMessage(senderId, "hello").Value;
        conversation.ClearDomainEvents();

        var reader = conversation.GetMember(readerId)!;
        reader.ClearDomainEvents();

        _currentUserService.UserId.Returns(readerId);
        _conversationRepository
            .GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _conversationRepository
            .GetMessageAsync(conversation.Id, message.Id, Arg.Any<CancellationToken>())
            .Returns(message);

        var sut = new MarkAsReadCommandHandler(
            _currentUserService,
            _conversationRepository,
            _unitOfWork
        );

        var result = await sut.Handle(
            new MarkAsReadCommand(conversation.Id, message.Id),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        reader.LastReadMessageId.Should().Be(message.Id);
        reader.DomainEvents.Should().ContainSingle();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}