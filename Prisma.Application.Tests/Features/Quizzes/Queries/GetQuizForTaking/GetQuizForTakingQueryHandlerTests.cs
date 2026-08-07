using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Queries.GetQuizForTaking;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetQuizForTaking;

public class GetQuizForTakingQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly GetQuizForTakingQueryHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly GetQuizForTakingQuery ValidQuery = new(QuizId: 1);

    public GetQuizForTakingQueryHandlerTests()
    {
        _currentUser.UserId.Returns(StudentId);

        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);

        _attemptRepository
            .FirstOrDefaultAsync(Arg.Any<StudentAttemptWithAnswersSpecification>(), Arg.Any<CancellationToken>())
            .Returns((QuizAttempt?)null);

        

        _handler = new GetQuizForTakingQueryHandler(_unitOfWork, _currentUser);
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

    private static Lesson CreateLesson(
        string firstName = "Ahmed",
        string lastName = "Mostafa",
        string subject = "Math"
        )
    {
        return new Lesson
        {
            Id = 1,
            Title = "Lesson",
            Teacher = new global::Prisma.Domain.Entities.UserAggregate.Teacher
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Subject = subject
            }
        };
    }

    private static Quiz CreateQuiz(
    int id = 1,
    int durationMinutes = 30,
    DateTimeOffset? availableFrom = null,
    DateTimeOffset? dueDate = null,
    QuizScope scope = QuizScope.ComprehensiveExam,
    int? lessonId = null,
    Lesson? lesson = null,
    List<QuestionLessonQuiz>? questions = null)
    {
        return new Quiz
        {
            Id = id,
            Title = "Quiz 1",
            TimeInMinutes = TimeSpan.FromMinutes(durationMinutes),
            AvailableFrom = availableFrom,
            DueDate = dueDate,
            Scope = scope,
            LessonId = lessonId,
            Lesson = lesson ?? CreateLesson(), 
            Questions = questions ?? new List<QuestionLessonQuiz>()
        };
    }

    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizForTakingSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    private void SetupExistingAttempt(QuizAttempt? attempt) =>
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
    public async Task Handle_WhenAvailableFromIsInFuture_ReturnsFailure()
    {
        // Arrange
        var quiz = CreateQuiz(availableFrom: DateTimeOffset.UtcNow.AddDays(1));
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("الاختبار غير متاح حاليًا", result.GetResultMessage());

        await _attemptRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<StudentAttemptWithAnswersSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region No existing attempt

    [Fact]
    public async Task Handle_WhenNoAttemptAndDueDatePassed_ReturnsFailure()
    {
        // Arrange
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1));
        SetupQuiz(quiz);
        SetupExistingAttempt(null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("انتهى موعد هذا الاختبار", result.GetResultMessage());

        _attemptRepository.DidNotReceive().Add(Arg.Any<QuizAttempt>());
    }

    [Fact]
    public async Task Handle_WhenNoAttemptAndWithinWindow_CreatesNewAttemptWithFullDuration()
    {
        // Arrange
        var quiz = CreateQuiz(durationMinutes: 30, dueDate: null);
        SetupQuiz(quiz);
        SetupExistingAttempt(null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _attemptRepository.Received(1).Add(Arg.Is<QuizAttempt>(a =>
            a.QuizId == quiz.Id &&
            a.StudentId == StudentId &&
            a.Status == QuizAttemptStatus.InProgress));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.Equal(TimeSpan.FromMinutes(30).TotalSeconds, result.Value!.RemainingSeconds, tolerance: 1);
    }

    #endregion

    #region Existing InProgress attempt (not expired)

    [Fact]
    public async Task Handle_WhenAttemptInProgressAndNotExpired_ReturnsExistingAttemptWithoutCreatingNewOne()
    {
        // Arrange
        var quiz = CreateQuiz(durationMinutes: 30);
        var attempt = new QuizAttempt
        {
            Id = 55,
            QuizId = quiz.Id,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10), // 20 min remaining
            Status = QuizAttemptStatus.InProgress
        };

        SetupQuiz(quiz);
        SetupExistingAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(55, result.Value!.AttemptId);
        Assert.InRange(result.Value.RemainingSeconds, 19 * 60, 20 * 60 + 5);

        _attemptRepository.DidNotReceive().Add(Arg.Any<QuizAttempt>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Existing attempt already submitted/graded

    [Theory]
    [InlineData(QuizAttemptStatus.Submitted)]
    [InlineData(QuizAttemptStatus.Graded)]
    public async Task Handle_WhenAttemptAlreadyFinal_ReturnsFailure(QuizAttemptStatus status)
    {
        // Arrange
        var quiz = CreateQuiz();
        var attempt = new QuizAttempt
        {
            Id = 1,
            QuizId = quiz.Id,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-1),
            Status = status
        };

        SetupQuiz(quiz);
        SetupExistingAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("سبق أن قمت بتسليم هذا الاختبار", result.GetResultMessage());
    }

    #endregion

    #region Auto-finalize on expired InProgress attempt

    [Fact]
    public async Task Handle_WhenInProgressAttemptTimeExpired_FinalizesAttemptAndReturnsExpiredMessage()
    {
        // Arrange: 30-minute quiz, started 45 minutes ago => already expired
        var quiz = CreateQuiz(durationMinutes: 30, scope: QuizScope.ComprehensiveExam, dueDate: null);
        var expiredAttempt = new QuizAttempt
        {
            Id = 1,
            QuizId = quiz.Id,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-45),
            Status = QuizAttemptStatus.InProgress,
            TabSwitchCount = 0,
            CopyPasteAttemptCount = 0
        };

        SetupQuiz(quiz);
        SetupExistingAttempt(expiredAttempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("انتهى وقت هذه المحاولة", result.GetResultMessage());

        // the old attempt was finalized (no answers => auto-graded with Degree 0)
        Assert.Equal(QuizAttemptStatus.Graded, expiredAttempt.Status);
        Assert.NotNull(expiredAttempt.SubmittedAt);

        // no new attempt should ever be created
        _attemptRepository.DidNotReceive().Add(Arg.Any<QuizAttempt>());

        // exactly one SaveChanges — the one triggered by the finalizer
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInProgressAttemptTimeExpiredAndDueDateAlsoPassed_StillReturnsExpiredAttemptMessage()
    {
        // Arrange - both conditions are true; the attempt-expiry check happens first
        var quiz = CreateQuiz(durationMinutes: 30, dueDate: DateTimeOffset.UtcNow.AddMinutes(-1));
        var expiredAttempt = new QuizAttempt
        {
            Id = 1,
            QuizId = quiz.Id,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-45),
            Status = QuizAttemptStatus.InProgress
        };

        SetupQuiz(quiz);
        SetupExistingAttempt(expiredAttempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("انتهى وقت هذه المحاولة", result.GetResultMessage());
        Assert.Equal(QuizAttemptStatus.Graded, expiredAttempt.Status);
    }

    #endregion

    #region Mapping

    [Fact]
    public async Task Handle_MapsMcqQuestionWithChoicesAndSavedAnswer()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz(questions: new List<QuestionLessonQuiz>
        {
            new() { QuestionId = question.Id, Question = question, Degree = 5m }
        });

        var attempt = new QuizAttempt
        {
            Id = 1,
            QuizId = quiz.Id,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<AttemptAnswer> { new() { QuestionId = question.Id, ChoiceId = 100 } }
        };

        SetupQuiz(quiz);
        SetupExistingAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!.Questions);
        Assert.Equal(2, dto.Choices!.Count);
        Assert.Equal(100, dto.SelectedChoiceId);
        Assert.Null(dto.SavedTextAnswer);
    }

    [Fact]
    public async Task Handle_MapsWrittenQuestionWithNullChoicesAndSavedText()
    {
        // Arrange
        var question = new WrittenQuestion { Id = 2, Title = "Explain X", Type = QuestionType.Written };
        var quiz = CreateQuiz(questions: new List<QuestionLessonQuiz>
        {
            new() { QuestionId = question.Id, Question = question, Degree = 10m }
        });

        var attempt = new QuizAttempt
        {
            Id = 1,
            QuizId = quiz.Id,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = QuizAttemptStatus.InProgress,
            Answers = new List<AttemptAnswer> { new() { QuestionId = question.Id, TextAnswer = "my draft answer" } }
        };

        SetupQuiz(quiz);
        SetupExistingAttempt(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!.Questions);

        dto.Choices.Should().BeNull();
        dto.SavedTextAnswer.Should().Be("my draft answer");
        Assert.Null(dto.SelectedChoiceId);
    }

    [Fact]
    public async Task Handle_MapsTeacherNameFromLessonTeacher()
    {
        // Arrange
        var teacher = new global::Prisma.Domain.Entities.UserAggregate.Teacher
        {
            Id = Guid.NewGuid(),
            FirstName = "Mona",
            LastName = "Kamal",
            Subject = "Physics"
        };


        var quiz = CreateQuiz(
            lesson: CreateLesson("Mona", "Kamal", "Physics"),
            dueDate: null);

        SetupQuiz(quiz);
        SetupExistingAttempt(null);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TeacherName.Should().Be("Mona Kamal");
    }

    #endregion
}