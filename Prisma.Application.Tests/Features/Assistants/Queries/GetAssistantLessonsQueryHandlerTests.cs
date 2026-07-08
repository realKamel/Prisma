using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assistants;

namespace Prisma.Application.Tests.Features.Assistants.Queries;

public class GetAssistantLessonsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Lesson, int> _lessonRepo;
    private readonly GetAssistantLessonsQueryHandler _handler;

    public GetAssistantLessonsQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _lessonRepo = Substitute.For<IRepository<Lesson, int>>();

        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        _handler = new GetAssistantLessonsQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var act = async () => await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");

        _unitOfWork.DidNotReceive().GetOrCreateRepository<Lesson, int>();
    }

    [Fact]
    public async Task Handle_HappyPath_MapsLessonsToDto()
    {
        // Arrange
        var lastUpdated = DateTimeOffset.UtcNow;
        var lessons = new List<Lesson>
        {
            new()
            {
                Id = 1,
                Title = "Algebra Basics",
                Price = 99.99m,
                Status = LessonStatus.Active,
                UpdatedAt = lastUpdated,
                CreatedAt = lastUpdated.AddDays(-10),
                Enrollments = new List<Enrollment> { new(), new(), new() },
                Sections = new List<Section> { new(), new() }
            }
        };

        _lessonRepo.ListAsync(Arg.Any<AssistantLessonsSpec>(), Arg.Any<CancellationToken>())
            .Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);

        var dto = result.Data.Single();
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
        _lessonRepo.ListAsync(Arg.Any<AssistantLessonsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<Lesson>());

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenLessonTitleIsNull_DefaultsToEmptyString()
    {
        // Arrange
        var lessons = new List<Lesson>
        {
            new()
            {
                Id = 2,
                Title = null,
                Price = 0m,
                Status = LessonStatus.Active,
                UpdatedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                Enrollments = null!,
                Sections = null!
            }
        };

        _lessonRepo.ListAsync(Arg.Any<AssistantLessonsSpec>(), Arg.Any<CancellationToken>())
            .Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        var dto = result.Data.Single();
        dto.Title.Should().Be(string.Empty);
        dto.StudentsCount.Should().Be(0);
        dto.ChaptersCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenUpdatedAtIsNull_FallsBackToCreatedAt()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddDays(-5);
        var lessons = new List<Lesson>
        {
            new()
            {
                Id = 3,
                Title = "Geometry",
                Price = 50m,
                Status = LessonStatus.Active,
                UpdatedAt = null,
                CreatedAt = createdAt,
                Enrollments = new List<Enrollment>(),
                Sections = new List<Section>()
            }
        };

        _lessonRepo.ListAsync(Arg.Any<AssistantLessonsSpec>(), Arg.Any<CancellationToken>())
            .Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetAssistantLessonsQuery(), CancellationToken.None);

        // Assert
        result.Data.Single().LastUpdatedAt.Should().Be(createdAt);
    }
}