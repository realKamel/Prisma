
using NSubstitute;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;
using Prisma.Application.Features.Quizzes.Specifications;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetTeacherQuizzesList;


public class GetTeacherQuizzesListQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly GetTeacherQuizzesListQueryHandler _handler;

    private static readonly GetTeacherQuizzesListQuery ValidQuery = new(QuizScope.ComprehensiveExam, null, null);

    public GetTeacherQuizzesListQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _handler = new GetTeacherQuizzesListQueryHandler(_unitOfWork);
    }

    #region Helpers

    private static QuizAttempt CreateAttempt(QuizAttemptStatus status, decimal degree = 0m) =>
        new() { Status = status, Degree = degree, StartedAt = DateTimeOffset.UtcNow.AddDays(-1) };

    private static TeacherQuizzesListProjection CreateQuizProjection(
        int id = 1,
        string title = "Quiz",
        decimal totalDegree = 100m,
        int durationMinutes = 30,
        int questionsCount = 5,
        int pendingGradingCount = 0,
        int submittedCount = 0,
        decimal? averageDegree = null,
        bool hasAttempts = false,
        bool hasUngradedAttempts = false) =>
        new()
        {
            QuizId = id,
            Title = title,
            TotalDegree = totalDegree,
            TimeInMinutes = TimeSpan.FromMinutes(durationMinutes),
            QuestionsCount = questionsCount,
            PendingGradingCount = pendingGradingCount,
            SubmittedCount = submittedCount,
            AverageDegree = averageDegree,
            HasAttempts = hasAttempts,
            HasUngradedAttempts = hasUngradedAttempts
        };

    private void SetupQuizzes(params TeacherQuizzesListProjection[] quizzes) =>
        _quizRepository
            .ListAsync(
                Arg.Any<TeacherQuizzesListSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(quizzes.ToList());


    #endregion

    #region Status computation

    [Fact]
    public async Task Handle_WhenNoAttempts_ReturnsActiveStatus()
    {
        // Arrange
        SetupQuizzes(CreateQuizProjection());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("active", Assert.Single(result.Value!.Items).Status);
    }

    [Fact]
    public async Task Handle_WhenAllAttemptsInProgress_ReturnsActiveStatus()
    {
        // Arrange
        var quiz = CreateQuizProjection(
       hasAttempts: true,
       hasUngradedAttempts: true);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("active", Assert.Single(result.Value!.Items).Status);
    }

    [Fact]
    public async Task Handle_WhenAtLeastOneAttemptSubmitted_ReturnsPendingGradingStatus()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            hasAttempts: true,
            hasUngradedAttempts: true,
            pendingGradingCount: 1,
            submittedCount: 2);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("pending_grading", item.Status);
        Assert.Equal(1, item.PendingGradingCount);
    }

    [Fact]
    public async Task Handle_WhenAllAttemptsGraded_ReturnsCompletedStatus()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            hasAttempts: true,
            hasUngradedAttempts: false,
            submittedCount: 2,
            averageDegree: 85m);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("completed", Assert.Single(result.Value!.Items).Status);
    }


    [Fact]
    public async Task Handle_WhenMixOfGradedAndInProgress_ReturnsActiveStatusNotCompleted()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            hasAttempts: true,
            hasUngradedAttempts: true,
            submittedCount: 1);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("active", Assert.Single(result.Value!.Items).Status);
    }

    #endregion

    #region Counts

    [Fact]
    public async Task Handle_ComputesSubmittedAndPendingCountsCorrectly()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            hasAttempts: true,
            hasUngradedAttempts: true,
            submittedCount: 3,
            pendingGradingCount: 1);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(3, item.SubmittedCount);
        Assert.Equal(1, item.PendingGradingCount);
    }

    #endregion

    #region Average score

    [Fact]
    public async Task Handle_ComputesAverageScoreFromGradedAttemptsOnly()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            totalDegree: 100m,
            hasAttempts: true,
            hasUngradedAttempts: true,
            averageDegree: 70m);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(70.0, Assert.Single(result.Value!.Items).AverageScore);
    }


    [Fact]
    public async Task Handle_WhenNoGradedAttempts_ReturnsNullAverageScore()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            hasAttempts: true,
            hasUngradedAttempts: true,
            pendingGradingCount: 1,
            averageDegree: null);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Null(Assert.Single(result.Value!.Items).AverageScore);
    }

    [Fact]
    public async Task Handle_WhenTotalDegreeIsZero_ReturnsNullAverageScoreEvenWithGradedAttempts()
    {
        // Arrange
        var quiz = CreateQuizProjection(
            totalDegree: 0m,
            hasAttempts: true,
            hasUngradedAttempts: false,
            averageDegree: 0m);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Null(Assert.Single(result.Value!.Items).AverageScore);
    }
    #endregion

    #region Status filtering

    [Fact]
    public async Task Handle_WhenStatusFilterProvided_ReturnsOnlyMatchingItemsButCorrectTotalCount()
    {
        // Arrange
        var activeQuiz = CreateQuizProjection(
             id: 1,
             hasAttempts: false,
             hasUngradedAttempts: false);

        var completedQuiz = CreateQuizProjection(
            id: 2,
            averageDegree: 90m,
            hasAttempts: true,
            hasUngradedAttempts: false);

        SetupQuizzes(activeQuiz, completedQuiz);

        var query = ValidQuery with { Status = "completed" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Value!.Items);
        Assert.Equal("completed", result.Value.Items.Single().Status);
        Assert.Equal(1, result.Value.TotalCount); // TotalCount reflects post-filter count
    }

    [Fact]
    public async Task Handle_WhenStatusFilterIsNull_ReturnsAllItems()
    {
        // Arrange
        SetupQuizzes(
            CreateQuizProjection(
             id: 1,
             hasAttempts: false,
             hasUngradedAttempts: false)
            ,
            CreateQuizProjection(
            id: 2,
            averageDegree: 90m,
            hasAttempts: true,
            hasUngradedAttempts: false));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);
    }

    #endregion

    #region Pagination

    [Fact]
    public async Task Handle_PaginatesResultsAccordingToPageAndPageSize()
    {
        // Arrange
        var quizzes = Enumerable.Range(1, 25)
            .Select(i => CreateQuizProjection(
                id: i,
                hasAttempts: false,
                hasUngradedAttempts: false))
            .ToArray();

        SetupQuizzes(quizzes);

        var query = ValidQuery with { Page = 2, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.Value!.Items.Count);
        Assert.Equal(11, result.Value.Items.First().QuizId);
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
    }
    [Fact]
    public async Task Handle_WhenPageIsZeroOrNegative_ClampsToPageOne()
    {
        // Arrange
        SetupQuizzes(
            CreateQuizProjection(
             id: 1,
             hasAttempts: false,
             hasUngradedAttempts: false),
             CreateQuizProjection(
                 id: 2, hasAttempts: false,
                 hasUngradedAttempts: false
                 )
             );
        var query = ValidQuery with { Page = 0 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Value!.Page);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenPageSizeExceedsMax_ClampsToOneHundred()
    {
        // Arrange
        SetupQuizzes(CreateQuizProjection());

        var query = ValidQuery with { PageSize = 500 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(100, result.Value!.PageSize);
    }
    [Fact]
    public async Task Handle_WhenPageSizeIsZeroOrNegative_ClampsToOne()
    {
        // Arrange
        SetupQuizzes(
            CreateQuizProjection(
             id: 1,
             hasAttempts: false,
             hasUngradedAttempts: false),
             CreateQuizProjection(
                 id: 2, hasAttempts: false,
                 hasUngradedAttempts: false
                 )
             );
        var query = ValidQuery with { PageSize = 0 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Value!.PageSize);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task Handle_WhenPageExceedsAvailableData_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        // Arrange
        SetupQuizzes(
            CreateQuizProjection(
             id: 1,
             hasAttempts: false,
             hasUngradedAttempts: false),
             CreateQuizProjection(
                 id: 2, hasAttempts: false,
                 hasUngradedAttempts: false
                 )
             );
        var query = ValidQuery with { Page = 5, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Value!.Items);
        Assert.Equal(2, result.Value.TotalCount);
    }

    #endregion

    #region Mapping & result

    [Fact]
    public async Task Handle_MapsBasicQuizFieldsCorrectly()
    {
        // Arrange
        SetupQuizzes(CreateQuizProjection(
            id: 7,
            title: "Midterm",
            totalDegree: 50m,
            durationMinutes: 45,
            questionsCount: 10));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(7, item.QuizId);
        Assert.Equal("Midterm", item.Title);
        Assert.Equal(50m, item.TotalDegree);
        Assert.Equal(45, item.DurationMinutes);
        Assert.Equal(10, item.QuestionsCount);
    }
    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsSucceededResult()
    {
        // Arrange
        SetupQuizzes(CreateQuizProjection());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert - relies on the implicit Result<T> conversion, not an explicit Success() call
        Assert.True(result.IsSuccess);
    }

    #endregion
}
