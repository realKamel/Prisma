using NSubstitute;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;
using Prisma.Application.Features.Quizzes.Specifications;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;

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

    private void SetupLessons(params LessonOptionDto[] lessons) =>
        _lessonRepository
            .ListAsync(Arg.Any<LessonsAvailableForQuizSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lessons.ToList());

    [Fact]
    public async Task Handle_WhenLessonsExist_MapsIdAndTitleCorrectly()
    {
        // Arrange
        SetupLessons(
    new LessonOptionDto
    {
        LessonId = 1,
        Title = "Algebra Basics"
    },
    new LessonOptionDto
    {
        LessonId = 2,
        Title = "Geometry Intro"
    });

        // Act
        var result = await _handler.Handle(new GetLessonsAvailableForQuizQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, l => l.LessonId == 1 && l.Title == "Algebra Basics");
        Assert.Contains(result.Value, l => l.LessonId == 2 && l.Title == "Geometry Intro");
    }


    [Fact]
    public async Task Handle_WhenNoLessonsAvailable_ReturnsEmptyList()
    {
        // Arrange
        SetupLessons();

        // Act
        var result = await _handler.Handle(new GetLessonsAvailableForQuizQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
