using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Queries.GetQuizResult;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetQuizResult;

public class GetQuizResultQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly GetQuizResultQueryHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly GetQuizResultQuery ValidQuery = new(QuizId: 1);

    public GetQuizResultQueryHandlerTests()
    {
        _currentUser.UserId.Returns(StudentId);
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);

        _handler = new GetQuizResultQueryHandler(_unitOfWork, _currentUser);
    }

    #region Helpers

    private static MCQQuestion CreateMcqQuestion(int id, int correctChoiceId) =>
        new()
        {
            Id = id,
            Title = "2+2?",
            Type = QuestionType.MCQ,
            Choices = new List<Choice>
            {
                new() { Id = correctChoiceId, Text = "4", IsCorrect = true },
                new() { Id = correctChoiceId + 1, Text = "5", IsCorrect = false }
            }
        };

    private static WrittenQuestion CreateWrittenQuestion(int id) =>
        new() { Id = id, Title = "Explain X", Type = QuestionType.Written, Answer = "model answer" };

    private static Quiz CreateQuiz(
        int id = 1,
        string title = "Quiz",
        decimal totalDegree = 100m,
        DateTimeOffset? dueDate = null,
        List<QuestionLessonQuiz>? questions = null) =>
        new()
        {
            Id = id,
            Title = title,
            TotalDegree = totalDegree,
            DueDate = dueDate,
            Questions = questions ?? new List<QuestionLessonQuiz>()
        };

    private static QuizAttempt CreateAttempt(
        QuizAttemptStatus status,
        decimal degree = 0m,
        DateTimeOffset? updatedAt = null,
        int tabSwitchCount = 0,
        int copyPasteCount = 0,
        List<AttemptAnswer>? answers = null) =>
        new()
        {
            Id = 1,
            StudentId = StudentId,
            Status = status,
            Degree = degree,
            UpdatedAt = updatedAt,
            TabSwitchCount = tabSwitchCount,
            CopyPasteAttemptCount = copyPasteCount,
            Answers = answers ?? new List<AttemptAnswer>()
        };

    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizWithQuestionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    private void SetupAttempt(QuizAttempt? attempt) =>
        _attemptRepository
            .FirstOrDefaultAsync(Arg.Any<StudentAttemptWithAnswersSpecification>(), Arg.Any<CancellationToken>())
            .Returns(attempt);

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenQuizNotFound_ReturnsFailure()
    {
        // Arrange
        SetupQuiz(null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("الاختبار غير موجود", result.GetResultMessage());
    }

    [Fact]
    public async Task Handle_WhenNoAttemptExists_ReturnsFailure()
    {
        // Arrange
        SetupQuiz(CreateQuiz());
        SetupAttempt(null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("لم يتم تسليم هذا الاختبار بعد", result.GetResultMessage());
    }

    [Fact]
    public async Task Handle_WhenAttemptStillInProgress_ReturnsFailure()
    {
        // Arrange
        SetupQuiz(CreateQuiz());
        SetupAttempt(CreateAttempt(QuizAttemptStatus.InProgress));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("لم يتم تسليم هذا الاختبار بعد", result.GetResultMessage());
    }

    #endregion

    #region Locked (due date not passed)

    [Fact]
    public async Task Handle_WhenDueDateNotPassed_ReturnsLockedStatusRegardlessOfAttemptOutcome()
    {
        // Arrange - attempt is already Graded, but due date hasn't passed yet
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(3));
        SetupQuiz(quiz);
        SetupAttempt(CreateAttempt(QuizAttemptStatus.Graded, degree: 90m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("locked", result.Value!.Status);
        Assert.Equal(quiz.DueDate, result.Value.AvailableAt);
        Assert.Null(result.Value.Score);
        Assert.Null(result.Value.Review);
    }

    [Fact]
    public async Task Handle_WhenDueDateNotPassedAndAttemptSubmitted_ReturnsLockedNotPending()
    {
        // Arrange
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(1));
        SetupQuiz(quiz);
        SetupAttempt(CreateAttempt(QuizAttemptStatus.Submitted));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("locked", result.Value!.Status);
    }

    #endregion

    #region Pending (due date passed, not yet fully graded)

    [Fact]
    public async Task Handle_WhenDueDatePassedAndAttemptSubmittedButNotGraded_ReturnsPendingStatus()
    {
        // Arrange
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1));
        SetupQuiz(quiz);

        var attempt = CreateAttempt(QuizAttemptStatus.Submitted, answers:
[
    new AttemptAnswer { QuestionId = 1, IsCorrect = true, Score = 5m },   // graded correct
    new AttemptAnswer { QuestionId = 2, IsCorrect = false, Score = 0m },  // graded incorrect
    new AttemptAnswer { QuestionId = 3, Score = null }                    // still pending (written, ungraded)
]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = result.Value!;
        Assert.Equal("pending", dto.Status);
        Assert.Equal(1, dto.CorrectCount);
        Assert.Equal(1, dto.WrongCount);
        Assert.Equal(1, dto.PendingCount);
        Assert.Null(dto.Score);
        Assert.Null(dto.Review);
    }

    [Fact]
    public async Task Handle_WhenNoDueDateSetAndAttemptSubmittedButNotGraded_TreatsAsDueDatePassedAndReturnsPending()
    {
        // Arrange - no DueDate means dueDatePassed defaults to true
        var quiz = CreateQuiz(dueDate: null);
        SetupQuiz(quiz);
        SetupAttempt(CreateAttempt(QuizAttemptStatus.Submitted));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("pending", result.Value!.Status);
    }

    #endregion

    #region Done (due date passed, fully graded)

    [Fact]
    public async Task Handle_WhenDueDatePassedAndAttemptGraded_ReturnsDoneStatusWithScoreAndReview()
    {
        // Arrange
        var quiz = CreateQuiz(totalDegree: 5m, dueDate: DateTimeOffset.UtcNow.AddDays(-1), questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = CreateMcqQuestion(1, correctChoiceId: 100), Degree = 5m }
        ]);
        SetupQuiz(quiz);

        var gradedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 5m, updatedAt: gradedAt, answers:
        [
            new AttemptAnswer { QuestionId = 1, ChoiceId = 100, IsCorrect = true, Score = 5m }
        ]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = result.Value!;
        Assert.Equal("done", dto.Status);
        Assert.Equal(5m, dto.Score);
        Assert.Equal(gradedAt, dto.GradedAt);
        Assert.Equal(0, dto.PendingCount); // hardcoded to 0, not computed, in the done branch
        Assert.Equal(1, dto.CorrectCount);
        Assert.Equal(0, dto.WrongCount);
        Assert.NotNull(dto.Review);
        Assert.Single(dto.Review!);
    }

    [Fact]
    public async Task Handle_WhenDone_PendingCountIsZeroEvenIfUngradedAnswersSomehowExist()
    {
        // Arrange - inconsistent data scenario: attempt is Graded but an answer still has Score == null
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1), questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = CreateWrittenQuestion(1), Degree = 10m }
        ]);
        SetupQuiz(quiz);

        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 0m, answers:
        [
            new AttemptAnswer { QuestionId = 1, Score = null }
        ]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.Value!.PendingCount); // hardcoded, doesn't reflect actual answer state
    }

    #endregion

    #region Review mapping - MCQ

    [Fact]
    public async Task Handle_ReviewForMcqQuestion_IncludesChoicesAndSelectedAnswer()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1), questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 5m }
        ]);
        SetupQuiz(quiz);

        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 0m, answers:
        [
            new AttemptAnswer { QuestionId = 1, ChoiceId = 101, IsCorrect = false, Score = 0m } // wrong answer
        ]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var reviewItem = Assert.Single(result.Value!.Review!);
        Assert.Equal(2, reviewItem.Choices!.Count);
        Assert.True(reviewItem.Choices.Single(c => c.ChoiceId == 100).IsCorrect);
        Assert.Equal(101, reviewItem.SelectedChoiceId);
        Assert.False(reviewItem.IsCorrect);
        Assert.Null(reviewItem.CorrectWrittenAnswer);
    }

    #endregion

    #region Review mapping - Written

    [Fact]
    public async Task Handle_ReviewForWrittenQuestion_IncludesModelAnswerWithNullChoices()
    {
        // Arrange
        var question = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1), questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 10m }
        ]);
        SetupQuiz(quiz);

        var attempt = CreateAttempt(QuizAttemptStatus.Graded, degree: 8m, answers:
        [
            new AttemptAnswer { QuestionId = 1, TextAnswer = "student's answer", Score = 8m }
        ]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var reviewItem = Assert.Single(result.Value!.Review!);
        Assert.Null(reviewItem.Choices);
        Assert.Equal("model answer", reviewItem.CorrectWrittenAnswer);
        Assert.Equal("student's answer", reviewItem.TextAnswer);
        Assert.Equal(8m, reviewItem.Score);
    }

    #endregion

    #region Review mapping - unanswered question

    [Fact]
    public async Task Handle_ReviewForQuestionWithNoAnswer_HasNullSelectionAndScore()
    {
        // Arrange - student left a question blank entirely
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1), questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = CreateMcqQuestion(1, 100), Degree = 5m }
        ]);
        SetupQuiz(quiz);

        var attempt = CreateAttempt(QuizAttemptStatus.Graded, answers: []); // no answers at all
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var reviewItem = Assert.Single(result.Value!.Review!);
        Assert.Null(reviewItem.SelectedChoiceId);
        Assert.Null(reviewItem.IsCorrect);
        Assert.Null(reviewItem.Score);
    }

    #endregion

    #region Security counts (currently unmapped — see note above)

    [Fact]
    public async Task Handle_MapsSecurityEventCountsFromAttempt()
    {
        // Arrange
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1));
        SetupQuiz(quiz);
        SetupAttempt(CreateAttempt(QuizAttemptStatus.Graded, tabSwitchCount: 4, copyPasteCount: 2));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(4, result.Value!.TabSwitchCount);
        Assert.Equal(2, result.Value.CopyPasteAttemptCount);
    }

    #endregion
}