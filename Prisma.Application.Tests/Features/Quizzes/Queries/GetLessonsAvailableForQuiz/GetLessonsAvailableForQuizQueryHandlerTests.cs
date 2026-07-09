using NSubstitute;
using Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;

public class GetLessonsAvailableForQuizQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepository = Substitute.For<IRepository<Lesson, int>>();
    private readonly GetLessonsAvailableForQuizQueryHandler _handler;

    public GetLessonsAvailableForQuizQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepository);
        _handler = new GetLessonsAvailableForQuizQueryHandler(_unitOfWork);
    }

    private void SetupLessons(params Lesson[] lessons) =>
        _lessonRepository
            .ListAsync(Arg.Any<LessonsAvailableForQuizSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lessons.ToList());

    [Fact]
    public async Task Handle_WhenLessonsExist_MapsIdAndTitleCorrectly()
    {
        // Arrange
        SetupLessons(
            new Lesson { Id = 1, Title = "Algebra Basics" },
            new Lesson { Id = 2, Title = "Geometry Intro" });

        // Act
        var result = await _handler.Handle(new GetLessonsAvailableForQuizQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data!.Count);
        Assert.Contains(result.Data, l => l.LessonId == 1 && l.Title == "Algebra Basics");
        Assert.Contains(result.Data, l => l.LessonId == 2 && l.Title == "Geometry Intro");
    }

    [Fact]
    public async Task Handle_WhenLessonTitleIsNull_MapsToEmptyString()
    {
        // Arrange
        SetupLessons(new Lesson { Id = 1, Title = null });

        // Act
        var result = await _handler.Handle(new GetLessonsAvailableForQuizQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(string.Empty, Assert.Single(result.Data!).Title);
    }

    [Fact]
    public async Task Handle_WhenNoLessonsAvailable_ReturnsEmptyList()
    {
        // Arrange
        SetupLessons();

        // Act
        var result = await _handler.Handle(new GetLessonsAvailableForQuizQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Data!);
    }
}
