
namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetStudentQuizzesList;

using Ardalis.Specification;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetStudentQuizzesList;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Application.Features.Quizzes.Specifications;

public class GetStudentQuizzesListQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Student, Guid> _studentRepository = Substitute.For<IRepository<Student, Guid>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepository = Substitute.For<IRepository<Enrollment, int>>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly GetStudentQuizzesListQueryHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    public GetStudentQuizzesListQueryHandlerTests()
    {
        _currentUser.UserId.Returns(StudentId);

        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepository);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepository);
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);

        // default: student has an academic year, no enrolled lessons unless overridden
        _studentRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Student, int?>>(), Arg.Any<CancellationToken>())
            .Returns((int?)1);

        _enrollmentRepository
            .ListAsync(Arg.Any<ISpecification<Enrollment, int?>>(), Arg.Any<CancellationToken>())
            .Returns(new List<int?>());

        _handler = new GetStudentQuizzesListQueryHandler(_unitOfWork, _currentUser);
    }

    #region Helpers

    private static AttemptProjection CreateAttempt(
    QuizAttemptStatus status,
    decimal degree = 0m,
    DateTimeOffset? submittedAt = null) =>
    new()
    {
        Id = 1,
        Status = status,
        Degree = degree,
        SubmittedAt = submittedAt ?? Now.AddMinutes(-5)
    };

    private static StudentQuizzesListProjection CreateQuiz(
    int id = 1,
    string title = "Quiz",
    decimal totalDegree = 100m,
    DateTimeOffset? availableFrom = null,
    DateTimeOffset? dueDate = null,
    int durationMinutes = 30,
    int questionsCount = 5,
    AttemptProjection? attempt = null)
    {
        return new StudentQuizzesListProjection
        {
            Id = id,
            Title = title,
            TotalDegree = totalDegree,
            AvailableFrom = availableFrom,
            DueDate = dueDate,
            DurationMinutes = durationMinutes,
            QuestionsCount = questionsCount,
            Attempt = attempt
        };
    }

    private void SetupQuizzes(params StudentQuizzesListProjection[] quizzes) =>
    _quizRepository
        .ListAsync(
            Arg.Any<StudentQuizzesSpecification>(),
            Arg.Any<CancellationToken>())
        .Returns(quizzes.ToList());

    private static StudentQuizListItemDto GetSingleItem(Result<StudentQuizzesListResponseDto> result) =>
        Assert.Single(result.Value!.Items);

    #endregion

    #region Status - no attempt

    [Fact]
    public async Task Handle_WhenNoAttemptAndAvailableFromInFuture_ReturnsUpcomingStatus()
    {
        // Arrange
        var quiz = CreateQuiz(availableFrom: Now.AddDays(1));
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("upcoming", item.Status);
        Assert.Null(item.Score);
    }

    [Fact]
    public async Task Handle_WhenNoAttemptAndDueDatePassed_ReturnsMissedStatus()
    {
        // Arrange
        var quiz = CreateQuiz(availableFrom: Now.AddDays(-5), dueDate: Now.AddDays(-1));
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("missed", item.Status);
    }

    [Fact]
    public async Task Handle_WhenNoAttemptAndOpenWindow_ReturnsNewStatus()
    {
        // Arrange
        var quiz = CreateQuiz(availableFrom: null, dueDate: Now.AddDays(5));
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("new", item.Status);
    }

    [Fact]
    public async Task Handle_WhenNoAttemptAndNoDatesSet_ReturnsNewStatus()
    {
        // Arrange
        var quiz = CreateQuiz(availableFrom: null, dueDate: null);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("new", item.Status);
    }

    #endregion

    #region Status - with attempt

    [Fact]
    public async Task Handle_WhenGradedAndDueDatePassed_ReturnsDoneStatusWithScore()
    {
        // Arrange
        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 85m, submittedAt: Now.AddDays(-2));
        var quiz = CreateQuiz(totalDegree: 100m, dueDate: Now.AddDays(-1), attempt: attempt);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("done", item.Status);
        Assert.Equal(85m, item.Score);
        Assert.Equal(attempt.SubmittedAt, item.SubmittedAt);
    }

    [Fact]
    public async Task Handle_WhenGradedButDueDateStillOpen_ReturnsPendingStatusWithoutScore()
    {
        // Arrange
        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 85m);
        var quiz = CreateQuiz(dueDate: Now.AddDays(3), attempt: attempt);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("pending", item.Status);
        Assert.Null(item.Score);
    }

    [Fact]
    public async Task Handle_WhenGradedAndNoDueDateSet_ReturnsDoneStatus()
    {
        // Arrange - no DueDate means dueDatePassed defaults to true
        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 90m);
        var quiz = CreateQuiz(dueDate: null, attempt: attempt);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("done", item.Status);
    }

    [Fact]
    public async Task Handle_WhenSubmittedButNotGraded_ReturnsPendingStatus()
    {
        // Arrange
        var attempt = CreateAttempt(QuizAttemptStatus.Submitted);
        var quiz = CreateQuiz(dueDate: Now.AddDays(-1), attempt: attempt); // even with due date passed
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("pending", item.Status);
        Assert.Null(item.Score);
    }

    [Fact]
    public async Task Handle_WhenInProgress_ReturnsPendingStatus()
    {
        // Arrange
        var attempt = CreateAttempt(QuizAttemptStatus.InProgress);
        var quiz = CreateQuiz(attempt: attempt);
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal("pending", item.Status);
    }

    #endregion

    #region Stats calculation

    [Fact]
    public async Task Handle_ComputesAverageAndBestScoreFromDoneItemsOnly()
    {
        // Arrange
        var doneQuiz1 = CreateQuiz(id: 1, totalDegree: 100m, dueDate: Now.AddDays(-1),
            attempt: CreateAttempt(QuizAttemptStatus.Graded, degree: 80m)); // 80%

        var doneQuiz2 = CreateQuiz(id: 2, totalDegree: 50m, dueDate: Now.AddDays(-1),
            attempt: CreateAttempt(QuizAttemptStatus.Graded, degree: 40m)); // 80%

        var doneQuiz3 = CreateQuiz(id: 3, totalDegree: 100m, dueDate: Now.AddDays(-1),
            attempt: CreateAttempt(QuizAttemptStatus.Graded, degree: 100m)); // 100%

        var pendingQuiz = CreateQuiz(id: 4, attempt: CreateAttempt(QuizAttemptStatus.InProgress));

        SetupQuizzes(doneQuiz1, doneQuiz2, doneQuiz3, pendingQuiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var stats = result.Value!.Stats;
        Assert.Equal(4, stats.Total);
        Assert.Equal(3, stats.DoneCount);
        Assert.Equal(1, stats.PendingCount);
        Assert.Equal(86.7, stats.AverageScorePercent); // (80+80+100)/3 = 86.66... rounded to 86.7
        Assert.Equal(100.0, stats.BestScorePercent);
    }

    [Fact]
    public async Task Handle_WhenNoDoneItems_ReturnsZeroForAverageAndBestScore()
    {
        // Arrange
        var quiz = CreateQuiz(availableFrom: null, dueDate: null); // "new"
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var stats = result.Value!.Stats;
        Assert.Equal(0, stats.AverageScorePercent);
        Assert.Equal(0, stats.BestScorePercent);
    }

    [Fact]
    public async Task Handle_WhenDoneItemHasZeroTotalDegree_ExcludesItFromScoreCalculation()
    {
        // Arrange - avoid division by zero; this quiz shouldn't count toward stats
        var quiz = CreateQuiz(totalDegree: 0m, dueDate: Now.AddDays(-1),
            attempt: CreateAttempt(QuizAttemptStatus.Graded, degree: 0m));
        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var stats = result.Value!.Stats;
        Assert.Equal(1, stats.DoneCount); // still counted as "done" status
        Assert.Equal(0, stats.AverageScorePercent); // but excluded from the average calc
    }

    [Fact]
    public async Task Handle_StatsReflectFullListRegardlessOfFilter()
    {
        // Arrange
        var doneQuiz = CreateQuiz(id: 1, totalDegree: 100m, dueDate: Now.AddDays(-1),
            attempt: CreateAttempt(QuizAttemptStatus.Graded, degree: 90m));
        var newQuiz = CreateQuiz(id: 2, availableFrom: null, dueDate: null);

        SetupQuizzes(doneQuiz, newQuiz);

        // Act - filter to only "done"
        var result = await _handler.Handle(new GetStudentQuizzesListQuery("done"), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Stats.Total); // stats computed on full unfiltered set
        Assert.Single(result.Value.Items); // but returned items are filtered
        Assert.Equal("done", result.Value.Items.Single().Status);
    }

    #endregion

    #region Filtering

    [Fact]
    public async Task Handle_WhenFilterIsNull_ReturnsAllItems()
    {
        // Arrange
        var quiz1 = CreateQuiz(id: 1, availableFrom: null, dueDate: null); // new
        var quiz2 = CreateQuiz(id: 2, attempt: CreateAttempt(QuizAttemptStatus.InProgress)); // pending
        SetupQuizzes(quiz1, quiz2);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenFilterIsAll_ReturnsAllItems()
    {
        // Arrange
        var quiz1 = CreateQuiz(id: 1, availableFrom: null, dueDate: null);
        var quiz2 = CreateQuiz(id: 2, attempt: CreateAttempt(QuizAttemptStatus.InProgress));
        SetupQuizzes(quiz1, quiz2);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery("all"), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenFilterIsSpecificStatus_ReturnsOnlyMatchingItems()
    {
        // Arrange
        var newQuiz = CreateQuiz(id: 1, availableFrom: null, dueDate: null);
        var pendingQuiz = CreateQuiz(id: 2, attempt: CreateAttempt(QuizAttemptStatus.InProgress));
        SetupQuizzes(newQuiz, pendingQuiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery("new"), CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("new", item.Status);
    }

    [Fact]
    public async Task Handle_WhenFilterMatchesNothing_ReturnsEmptyItemsButFullStats()
    {
        // Arrange
        var newQuiz = CreateQuiz(availableFrom: null, dueDate: null);
        SetupQuizzes(newQuiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery("done"), CancellationToken.None);

        // Assert
        Assert.Empty(result.Value!.Items);
        Assert.Equal(1, result.Value.Stats.Total);
    }

    #endregion

    #region Mapping

    [Fact]
    public async Task Handle_MapsQuizFieldsToDtoCorrectly()
    {
        // Arrange
        var quiz = CreateQuiz(
            id: 42,
            title: "Algebra Basics",
            totalDegree: 75m,
            durationMinutes: 45,
            questionsCount: 8,
            availableFrom: null,
            dueDate: null);

        SetupQuizzes(quiz);

        // Act
        var result = await _handler.Handle(new GetStudentQuizzesListQuery(null), CancellationToken.None);

        // Assert
        var item = GetSingleItem(result);
        Assert.Equal(42, item.QuizId);
        Assert.Equal("Algebra Basics", item.Title);
        Assert.Equal(75m, item.TotalDegree);
        Assert.Equal(45, item.DurationMinutes);
        Assert.Equal(8, item.QuestionsCount);
    }

    #endregion
}
