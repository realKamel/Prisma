
using NSubstitute;
using Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizzesList;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

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

    private static Quiz CreateQuiz(
        int id = 1,
        string title = "Quiz",
        decimal totalDegree = 100m,
        int durationMinutes = 30,
        int questionsCount = 5,
        IEnumerable<QuizAttempt>? attempts = null) =>
        new()
        {
            Id = id,
            Title = title,
            TotalDegree = totalDegree,
            TimeInMinutes = TimeSpan.FromMinutes(durationMinutes),
            Questions = Enumerable.Range(1, questionsCount).Select(_ => new QuestionLessonQuiz()).ToList(),
            Attempts = attempts?.ToList() ?? new List<QuizAttempt>()
        };

    private void SetupQuizzes(params Quiz[] quizzes) =>
        _quizRepository
            .ListAsync(Arg.Any<TeacherQuizzesSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quizzes.ToList());

    #endregion

    #region Status computation

    [Fact]
    public async Task Handle_WhenNoAttempts_ReturnsActiveStatus()
    {
        // Arrange
        SetupQuizzes(CreateQuiz());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("active", Assert.Single(result.Value!.Items).Status);
    }

    [Fact]
    public async Task Handle_WhenAllAttemptsInProgress_ReturnsActiveStatus()
    {
        // Arrange
        var quiz = CreateQuiz(attempts: [CreateAttempt(QuizAttemptStatus.InProgress)]);
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
        var quiz = CreateQuiz(attempts:
        [
            CreateAttempt(QuizAttemptStatus.Graded, degree: 90m),
            CreateAttempt(QuizAttemptStatus.Submitted)
        ]);
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
        var quiz = CreateQuiz(attempts:
        [
            CreateAttempt(QuizAttemptStatus.Graded, degree: 80m),
            CreateAttempt(QuizAttemptStatus.Graded, degree: 90m)
        ]);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("completed", Assert.Single(result.Value!.Items).Status);
    }

    [Fact]
    public async Task Handle_WhenMixOfGradedAndInProgress_ReturnsActiveStatusNotCompleted()
    {
        // Arrange - "completed" requires ALL attempts graded; InProgress here means not all are
        var quiz = CreateQuiz(attempts:
        [
            CreateAttempt(QuizAttemptStatus.Graded, degree: 80m),
            CreateAttempt(QuizAttemptStatus.InProgress)
        ]);
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
        var quiz = CreateQuiz(attempts:
        [
            CreateAttempt(QuizAttemptStatus.Graded, degree: 80m),
            CreateAttempt(QuizAttemptStatus.Graded, degree: 90m),
            CreateAttempt(QuizAttemptStatus.Submitted),
            CreateAttempt(QuizAttemptStatus.InProgress)
        ]);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(3, item.SubmittedCount); // 2 Graded + 1 Submitted
        Assert.Equal(1, item.PendingGradingCount); // only Submitted
    }

    #endregion

    #region Average score

    [Fact]
    public async Task Handle_ComputesAverageScoreFromGradedAttemptsOnly()
    {
        // Arrange
        var quiz = CreateQuiz(totalDegree: 100m, attempts:
        [
            CreateAttempt(QuizAttemptStatus.Graded, degree: 80m),  // 80%
            CreateAttempt(QuizAttemptStatus.Graded, degree: 60m),  // 60%
            CreateAttempt(QuizAttemptStatus.Submitted, degree: 0m) // excluded
        ]);
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
        var quiz = CreateQuiz(attempts: [CreateAttempt(QuizAttemptStatus.Submitted)]);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Null(Assert.Single(result.Value!.Items).AverageScore);
    }

    [Fact]
    public async Task Handle_WhenTotalDegreeIsZero_ReturnsNullAverageScoreEvenWithGradedAttempts()
    {
        // Arrange - avoid division by zero
        var quiz = CreateQuiz(totalDegree: 0m, attempts: [CreateAttempt(QuizAttemptStatus.Graded, degree: 0m)]);
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
        var activeQuiz = CreateQuiz(id: 1);
        var completedQuiz = CreateQuiz(id: 2, attempts: [CreateAttempt(QuizAttemptStatus.Graded, degree: 90m)]);
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
        SetupQuizzes(CreateQuiz(id: 1), CreateQuiz(id: 2, attempts: [CreateAttempt(QuizAttemptStatus.Graded, degree: 50m)]));

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
        var quizzes = Enumerable.Range(1, 25).Select(i => CreateQuiz(id: i)).ToArray();
        SetupQuizzes(quizzes);

        var query = ValidQuery with { Page = 2, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.Value!.Items.Count);
        Assert.Equal(11, result.Value.Items.First().QuizId); // second page starts at item 11
        Assert.Equal(25, result.Value.TotalCount); // total unaffected by paging
        Assert.Equal(2, result.Value.Page);
        Assert.Equal(10, result.Value.PageSize);
    }

    [Fact]
    public async Task Handle_WhenPageIsZeroOrNegative_ClampsToPageOne()
    {
        // Arrange
        SetupQuizzes(CreateQuiz(id: 1), CreateQuiz(id: 2));
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
        SetupQuizzes(CreateQuiz());
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
        SetupQuizzes(CreateQuiz(id: 1), CreateQuiz(id: 2));
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
        SetupQuizzes(CreateQuiz(id: 1), CreateQuiz(id: 2));
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
        SetupQuizzes(CreateQuiz(id: 7, title: "Midterm", totalDegree: 50m, durationMinutes: 45, questionsCount: 10));

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
        SetupQuizzes(CreateQuiz());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert - relies on the implicit Result<T> conversion, not an explicit Success() call
        Assert.True(result.IsSuccess);
    }

    #endregion
}
