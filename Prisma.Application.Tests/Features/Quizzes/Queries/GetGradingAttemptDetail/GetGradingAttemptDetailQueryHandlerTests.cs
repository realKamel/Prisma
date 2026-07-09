
using NSubstitute;
using Prisma.Application.Features.Quizzes.Queries.GetGradingAttemptDetail;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetGradingAttemptDetail;

public class GetGradingAttemptDetailQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly GetGradingAttemptDetailQueryHandler _handler;

    private static readonly GetGradingAttemptDetailQuery ValidQuery = new(AttemptId: 1);

    public GetGradingAttemptDetailQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);
        _handler = new GetGradingAttemptDetailQueryHandler(_unitOfWork);
    }

    #region Helpers

    private static Student CreateStudent(string firstName = "Sara", string lastName = "Ali") =>
        new() { Id = Guid.NewGuid(), FirstName = firstName, LastName = lastName };

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
        string title = "Quiz",
        decimal totalDegree = 100m,
        List<QuestionLessonQuiz>? questions = null) =>
        new()
        {
            Id = 1,
            Title = title,
            TotalDegree = totalDegree,
            Questions = questions ?? new List<QuestionLessonQuiz>()
        };

    private static QuizAttempt CreateAttempt(
        int id = 1,
        Student? student = null,
        Quiz? quiz = null,
        QuizAttemptStatus status = QuizAttemptStatus.Submitted,
        decimal degree = 0m,
        decimal penaltyScore = 0m,
        DateTimeOffset? submittedAt = null,
        int tabSwitchCount = 0,
        int copyPasteCount = 0,
        List<AttemptAnswer>? answers = null) =>
        new()
        {
            Id = id,
            Student = student ?? CreateStudent(),
            Quiz = quiz ?? CreateQuiz(),
            Status = status,
            Degree = degree,
            PenaltyScore = penaltyScore,
            SubmittedAt = submittedAt,
            TabSwitchCount = tabSwitchCount,
            CopyPasteAttemptCount = copyPasteCount,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-1),
            Answers = answers ?? new List<AttemptAnswer>()
        };

    private void SetupAttempt(QuizAttempt? attempt) =>
        _attemptRepository
            .FirstOrDefaultAsync(Arg.Any<AttemptDetailForGradingSpecification>(), Arg.Any<CancellationToken>())
            .Returns(attempt);

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenAttemptNotFound_ReturnsFailure()
    {
        // Arrange
        SetupAttempt(null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("المحاولة غير موجودة", result.Message);
    }

    [Fact]
    public async Task Handle_WhenAttemptStillInProgress_ReturnsFailure()
    {
        // Arrange
        SetupAttempt(CreateAttempt(status: QuizAttemptStatus.InProgress));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("الطالب لسه في الاختبار", result.Message);
    }

    #endregion

    #region Basic mapping

    [Fact]
    public async Task Handle_MapsStudentNameQuizTitleAndDegrees()
    {
        // Arrange
        var student = CreateStudent(firstName: "Mona", lastName: "Kamal");
        var quiz = CreateQuiz(title: "Midterm", totalDegree: 50m);
        var attempt = CreateAttempt(student: student, quiz: quiz, penaltyScore: 5m);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = result.Data!;
        Assert.Equal("Mona Kamal", dto.StudentName);
        Assert.Equal("Midterm", dto.QuizTitle);
        Assert.Equal(50m, dto.TotalDegree);
        Assert.Equal(5m, dto.PenaltyScore);
    }

    [Fact]
    public async Task Handle_WhenStatusIsSubmitted_ScoreIsNullAndStatusStringIsSubmitted()
    {
        // Arrange
        SetupAttempt(CreateAttempt(status: QuizAttemptStatus.Submitted, degree: 40m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("submitted", result.Data!.Status);
        Assert.Null(result.Data.Score);
    }

    [Fact]
    public async Task Handle_WhenStatusIsGraded_ScoreReflectsDegreeAndStatusStringIsGraded()
    {
        // Arrange
        SetupAttempt(CreateAttempt(status: QuizAttemptStatus.Graded, degree: 88m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("graded", result.Data!.Status);
        Assert.Equal(88m, result.Data.Score);
    }

    #endregion

    #region HeldForSecurityReview

    [Fact]
    public async Task Handle_WhenTabSwitchCountGreaterThanZero_HeldForSecurityReviewIsTrue()
    {
        // Arrange
        SetupAttempt(CreateAttempt(tabSwitchCount: 1));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.Data!.HeldForSecurityReview);
    }

    [Fact]
    public async Task Handle_WhenCopyPasteCountGreaterThanZero_HeldForSecurityReviewIsTrue()
    {
        // Arrange
        SetupAttempt(CreateAttempt(copyPasteCount: 1));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.Data!.HeldForSecurityReview);
    }

    [Fact]
    public async Task Handle_WhenNoSecurityEvents_HeldForSecurityReviewIsFalse()
    {
        // Arrange
        SetupAttempt(CreateAttempt(tabSwitchCount: 0, copyPasteCount: 0));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.Data!.HeldForSecurityReview);
    }

    [Fact]
    public async Task Handle_WhenSecurityEventsExistButWrittenAnswersStillPending_HeldForSecurityReviewIsStillTrue()
    {
        // Arrange - unlike GetGradingListQueryHandler, this handler doesn't require full grading first
        var attempt = CreateAttempt(
            tabSwitchCount: 2,
            answers: [new AttemptAnswer { QuestionId = 1, Score = null }]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.Data!.HeldForSecurityReview);
    }

    #endregion

    #region HeldForManualGrading

    [Fact]
    public async Task Handle_WhenAnyAnswerHasNullScore_HeldForManualGradingIsTrue()
    {
        // Arrange
        var attempt = CreateAttempt(answers:
        [
            new AttemptAnswer { QuestionId = 1, Score = 5m },
            new AttemptAnswer { QuestionId = 2, Score = null }
        ]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.Data!.HeldForManualGrading);
    }

    [Fact]
    public async Task Handle_WhenAllAnswersHaveScores_HeldForManualGradingIsFalse()
    {
        // Arrange
        var attempt = CreateAttempt(answers:
        [
            new AttemptAnswer { QuestionId = 1, Score = 5m },
            new AttemptAnswer { QuestionId = 2, Score = 0m }
        ]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.Data!.HeldForManualGrading);
    }

    [Fact]
    public async Task Handle_WhenNoAnswersAtAll_HeldForManualGradingIsFalse()
    {
        // Arrange - Any() on an empty collection is false, so no answers means nothing pending
        SetupAttempt(CreateAttempt(answers: []));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.Data!.HeldForManualGrading);
    }

    #endregion

    #region Questions mapping - MCQ

    [Fact]
    public async Task Handle_QuestionMappingForMcq_IncludesChoicesWithIsCorrectAndSelectedAnswer()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 5m }
        ]);
        var answer = new AttemptAnswer { Id = 50, QuestionId = 1, ChoiceId = 100, IsCorrect = true, Score = 5m };
        var attempt = CreateAttempt(quiz: quiz, answers: [answer]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var q = Assert.Single(result.Data!.Questions);
        Assert.Equal(50, q.AnswerId);
        Assert.Equal(2, q.Choices!.Count);
        Assert.True(q.Choices.Single(c => c.ChoiceId == 100).IsCorrect);
        Assert.Equal(100, q.SelectedChoiceId);
        Assert.True(q.IsCorrect);
        Assert.Equal(5m, q.Score);
        Assert.Null(q.ModelAnswer);
    }

    #endregion

    #region Questions mapping - Written

    [Fact]
    public async Task Handle_QuestionMappingForWritten_IncludesModelAnswerWithNullChoices()
    {
        // Arrange
        var question = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz(questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 10m }
        ]);
        var answer = new AttemptAnswer { Id = 51, QuestionId = 1, TextAnswer = "student's response", Score = null };
        var attempt = CreateAttempt(quiz: quiz, answers: [answer]);
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var q = Assert.Single(result.Data!.Questions);
        Assert.Null(q.Choices);
        Assert.Equal("model answer", q.ModelAnswer);
        Assert.Equal("student's response", q.TextAnswer);
        Assert.Null(q.Score);
        Assert.Null(q.IsCorrect);
    }

    #endregion

    #region Questions mapping - no answer for question

    [Fact]
    public async Task Handle_WhenQuestionHasNoAnswer_AnswerIdDefaultsToZero()
    {
        // Arrange - student left this question entirely blank
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 5m }
        ]);
        var attempt = CreateAttempt(quiz: quiz, answers: []); // no answers submitted
        SetupAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var q = Assert.Single(result.Data!.Questions);
        Assert.Equal(0, q.AnswerId);
        Assert.Null(q.SelectedChoiceId);
        Assert.Null(q.Score);
        Assert.Null(q.IsCorrect);
    }

    #endregion

    #region Degree source

    [Fact]
    public async Task Handle_MapsQuestionDegreeFromLinkNotFromQuestionEntity()
    {
        // Arrange - confirms Degree comes from QuestionLessonQuiz, consistent with other handlers
        var question = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz(questions:
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 20m }
        ]);
        SetupAttempt(CreateAttempt(quiz: quiz));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(20m, Assert.Single(result.Data!.Questions).Degree);
    }

    #endregion
}