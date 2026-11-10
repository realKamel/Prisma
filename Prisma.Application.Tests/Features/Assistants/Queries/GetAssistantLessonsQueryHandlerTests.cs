using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Assistants.Queries;

public class GetAssistantLessonsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Lesson, int> _lessonRepo;
    private readonly IRepository<Assistant, Guid> _assistantRepo;
    private readonly IIdentityService _identityService;
    private readonly GetAssistantLessonsQueryHandler _handler;

    private static readonly Guid AssistantId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();

    public GetAssistantLessonsQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
        _assistantRepo = Substitute.For<IRepository<Assistant, Guid>>();
        _identityService = Substitute.For<IIdentityService>();

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<Assistant, Guid>().Returns(_assistantRepo);
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(AssistantId);
        _assistantRepo
            .GetByIdAsync(AssistantId, Arg.Any<CancellationToken>())
            .Returns(new Assistant { Id = AssistantId, TeacherId = TeacherId });

        _handler = new GetAssistantLessonsQueryHandler(
            _unitOfWork,
            _currentUserService,
            _identityService
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.IsAuthenticated.Returns(false);

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);

        _unitOfWork.DidNotReceive().GetOrCreateRepository<Lesson, int>();
    }

    [Fact]
    public async Task Handle_WhenAssistantHasNoTeacher_ReturnsError()
    {
        // Arrange
        _assistantRepo
            .GetByIdAsync(AssistantId, Arg.Any<CancellationToken>())
            .Returns(new Assistant { Id = AssistantId, TeacherId = null });

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain("Assistant not found");

        await _lessonRepo
            .DidNotReceive()
            .ListAsync(
                Arg.Any<LessonWithProjectionSpec<AssistantLessonDto>>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_HappyPath_MapsLessonsToDto()
    {
        // Arrange
        var lastUpdated = DateTimeOffset.UtcNow;
        var lessons = new List<AssistantLessonDto>
        {
            new()
            {
                Id = 1,
                Title = "Algebra Basics",
                Price = 99.99m,
                Status = "active",
                LastUpdatedAt = lastUpdated,
                StudentsCount = 3,
                ChaptersCount = 2,
            },
        };

        _lessonRepo
            .ListAsync(
                Arg.Any<LessonWithProjectionSpec<AssistantLessonDto>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value.Single();
        dto.Id.Should().Be(1);
        dto.Title.Should().Be("Algebra Basics");
        dto.Price.Should().Be(99.99m);
        dto.StudentsCount.Should().Be(3);
        dto.ChaptersCount.Should().Be(2);
        dto.LastUpdatedAt.Should().Be(lastUpdated);
        dto.Status.Should().Be("active");
    }

    [Fact]
    public async Task Handle_WhenNoLessonsExist_ReturnsEmptyList()
    {
        // Arrange
        _lessonRepo
            .ListAsync(
                Arg.Any<LessonWithProjectionSpec<AssistantLessonDto>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<AssistantLessonDto>());

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_QueriesLessonsForAssistantsTeacher()
    {
        // Arrange
        _lessonRepo
            .ListAsync(
                Arg.Any<LessonWithProjectionSpec<AssistantLessonDto>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<AssistantLessonDto>());

        // Act
        await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        await _lessonRepo
            .Received(1)
            .ListAsync(
                Arg.Any<LessonWithProjectionSpec<AssistantLessonDto>>(),
                Arg.Any<CancellationToken>()
            );
        await _assistantRepo.Received(1).GetByIdAsync(AssistantId, Arg.Any<CancellationToken>());
    }
}
