using System;
using System.Collections.Generic;
using System.Text;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.SubmitQuizAttempt;


public class SubmitQuizAttemptCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepository = Substitute.For<IRepository<Enrollment, int>>();
    private readonly SubmitQuizAttemptCommandHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly SubmitQuizAttemptCommand ValidCommand = new(AttemptId: 1);

    public SubmitQuizAttemptCommandHandlerTests()
    {
        _currentUser.UserId.Returns(StudentId);

        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepository);

        // QuizFinalizer looks this up when scope is LessonQuiz; default to "not found" so it's a no-op
        _enrollmentRepository
            .FirstOrDefaultAsync(Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);

        _handler = new SubmitQuizAttemptCommandHandler(_unitOfWork, _currentUser);
    }

    #region Helpers

    private static MCQQuestion CreateMcqQuestion(int id, int correctChoiceId) =>
        new()
        {
            Id = id,
            Title = "Q",
            Type = QuestionType.MCQ,
            Choices = new List<Choice>
            {
                new() { Id = correctChoiceId, Text = "Correct", IsCorrect = true },
                new() { Id = correctChoiceId + 1, Text = "Wrong", IsCorrect = false }
            }
        };

    private static WrittenQuestion CreateWrittenQuestion(int id) =>
        new() { Id = id, Title = "Q", Type = QuestionType.Written };

    private static Quiz CreateQuiz(
        int id = 10,
        decimal totalDegree = 100m,
        int durationMinutes = 30,
        List<QuestionLessonQuiz>? questions = null,
        QuizScope scope = QuizScope.ComprehensiveExam) =>
        new()
        {
            Id = id,
            TotalDegree = totalDegree,
            TimeInMinutes = TimeSpan.FromMinutes(durationMinutes),
            Scope = scope,
            Questions = questions ?? new List<QuestionLessonQuiz>()
        };

    private static QuizAttempt CreateAttempt(
        int id = 1,
        int quizId = 10,
        QuizAttemptStatus status = QuizAttemptStatus.InProgress,
        DateTimeOffset? startedAt = null,
        List<AttemptAnswer>? answers = null) =>
        new()
        {
            Id = id,
            QuizId = quizId,
            StudentId = StudentId,
            Status = status,
            StartedAt = startedAt ?? DateTimeOffset.UtcNow.AddMinutes(-5),
            Answers = answers ?? new List<AttemptAnswer>()
        };

    private void SetupAttempt(QuizAttempt? attempt) =>
        _attemptRepository
            .FirstOrDefaultAsync(Arg.Any<AttemptByIdAndStudentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(attempt);

    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizWithQuestionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenAttemptNotFound_ReturnsFailure()
    {
        // Arrange
        SetupAttempt(null);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("المحاولة غير موجودة", result.Message);

        await _quizRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<QuizWithQuestionsSpecification>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(QuizAttemptStatus.Submitted)]
    [InlineData(QuizAttemptStatus.Graded)]
    public async Task Handle_WhenAttemptAlreadySubmittedOrGraded_ReturnsFailure(QuizAttemptStatus status)
    {
        // Arrange
        SetupAttempt(CreateAttempt(status: status));

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("تم تسليم هذا الاختبار من قبل", result.Message);

        await _quizRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<QuizWithQuestionsSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Successful submission

    [Fact]
    public async Task Handle_WhenAllMcqAnsweredCorrectly_ReturnsGradedStatusWithScore()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(totalDegree: 5m, questions:
        [
            new QuestionLessonQuiz { QuestionId = question.Id, Question = question, Degree = 5m }
        ]);
        var attempt = CreateAttempt(quizId: quiz.Id, answers:
        [
            new AttemptAnswer { QuestionId = question.Id, ChoiceId = 100 }
        ]);

        SetupAttempt(attempt);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("graded", result.Data!.Status);
        Assert.Equal(5m, result.Data.Score);
        Assert.Equal(5m, result.Data.TotalDegree);
        Assert.Equal(QuizAttemptStatus.Graded, attempt.Status);
    }

    [Fact]
    public async Task Handle_WhenQuizHasPendingWrittenQuestion_ReturnsSubmittedStatusWithNullScore()
    {
        // Arrange
        var written = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz(totalDegree: 10m, questions:
        [
            new QuestionLessonQuiz { QuestionId = written.Id, Question = written, Degree = 10m }
        ]);
        var attempt = CreateAttempt(quizId: quiz.Id, answers:
        [
            new AttemptAnswer { QuestionId = written.Id, TextAnswer = "my answer", Score = null }
        ]);

        SetupAttempt(attempt);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("submitted", result.Data!.Status);
        Assert.Null(result.Data.Score);
        Assert.Equal(10m, result.Data.TotalDegree); // total degree always returned, even when ungraded
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
    }

    [Fact]
    public async Task Handle_WhenSecurityViolationOccurred_ReturnsSubmittedStatusDespiteFullAutoGrading()
    {
        // Arrange - all MCQ, fully answerable automatically, but held for review due to tab switches
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(totalDegree: 5m, questions:
        [
            new QuestionLessonQuiz { QuestionId = question.Id, Question = question, Degree = 5m }
        ]);
        var attempt = CreateAttempt(quizId: quiz.Id, answers:
        [
            new AttemptAnswer { QuestionId = question.Id, ChoiceId = 100 }
        ]);
        attempt.TabSwitchCount = 3;

        SetupAttempt(attempt);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("submitted", result.Data!.Status); // held for review, not graded
        Assert.Null(result.Data.Score); // Status != Graded, so DTO reports null even though Degree was computed internally
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
    }

    [Fact]
    public async Task Handle_WhenSubmittedWellPastHardDeadline_StillSucceedsAsAutoSubmit()
    {
        // Arrange - started 2 hours ago on a 30-minute quiz, way past the 10s grace period
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(durationMinutes: 30, totalDegree: 5m, questions:
        [
            new QuestionLessonQuiz { QuestionId = question.Id, Question = question, Degree = 5m }
        ]);
        var attempt = CreateAttempt(
            quizId: quiz.Id,
            startedAt: DateTimeOffset.UtcNow.AddHours(-2),
            answers: [new AttemptAnswer { QuestionId = question.Id, ChoiceId = 100 }]);

        SetupAttempt(attempt);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert - no rejection; treated as a normal (late) submission
        Assert.True(result.Succeeded);
        Assert.Equal("graded", result.Data!.Status);
    }

    [Fact]
    public async Task Handle_AlwaysReturnsQuizTotalDegreeRegardlessOfOutcome()
    {
        // Arrange
        var quiz = CreateQuiz(totalDegree: 42.5m);
        var attempt = CreateAttempt(quizId: quiz.Id);

        SetupAttempt(attempt);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.Equal(42.5m, result.Data!.TotalDegree);
    }

    #endregion
}
