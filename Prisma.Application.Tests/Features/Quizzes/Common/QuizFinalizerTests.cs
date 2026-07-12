
using NSubstitute;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Common;


public class QuizFinalizerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Enrollment, int> _enrollmentRepository = Substitute.For<IRepository<Enrollment, int>>();

    private static readonly Guid StudentId = Guid.NewGuid();

    public QuizFinalizerTests()
    {
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepository);

        _enrollmentRepository
            .FirstOrDefaultAsync(Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Enrollment?)null);
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
        List<(Domain.Entities.QuizAggregate.Question Question, decimal Degree)> questions,
        QuizScope scope = QuizScope.ComprehensiveExam,
        int? lessonId = null) =>
        new()
        {
            Id = 1,
            Scope = scope,
            LessonId = lessonId,
            Questions = questions.Select(q => new QuestionLessonQuiz
            {
                QuestionId = q.Question.Id,
                Question = q.Question,
                Degree = q.Degree
            }).ToList()
        };

    private static QuizAttempt CreateAttempt(
        List<AttemptAnswer>? answers = null,
        int tabSwitchCount = 0,
        int copyPasteCount = 0) =>
        new()
        {
            Id = 1,
            StudentId = StudentId,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Status = QuizAttemptStatus.InProgress,
            TabSwitchCount = tabSwitchCount,
            CopyPasteAttemptCount = copyPasteCount,
            Answers = answers ?? new List<AttemptAnswer>()
        };

    #endregion

    #region FinalizeAttempt - MCQ grading

    [Fact]
    public async Task FinalizeAttempt_WhenMcqAnswerIsCorrect_MarksCorrectAndAwardsFullDegree()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(question, 5m)]);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.True(answer.IsCorrect);
        Assert.Equal(5m, answer.Score);
        Assert.Equal(5m, attempt.Degree);
        Assert.Equal(QuizAttemptStatus.Graded, attempt.Status);
    }

    [Fact]
    public async Task FinalizeAttempt_WhenMcqAnswerIsIncorrect_MarksIncorrectWithZeroScore()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(question, 5m)]);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 101 }; // wrong choice
        var attempt = CreateAttempt([answer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.False(answer.IsCorrect);
        Assert.Equal(0m, answer.Score);
        Assert.Equal(0m, attempt.Degree);
    }

    [Fact]
    public async Task FinalizeAttempt_WhenMcqHasNoAnswer_SkipsQuestionWithoutCountingItOrBlockingGrading()
    {
        // Arrange - student left this MCQ blank
        var answeredQuestion = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var unansweredQuestion = CreateMcqQuestion(id: 2, correctChoiceId: 200);
        var quiz = CreateQuiz([(answeredQuestion, 5m), (unansweredQuestion, 5m)]);

        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(5m, attempt.Degree); // only the answered question counts
        Assert.Equal(QuizAttemptStatus.Graded, attempt.Status); // blank MCQ doesn't block grading
    }

    #endregion

    #region FinalizeAttempt - Written questions

    [Fact]
    public async Task FinalizeAttempt_WhenWrittenAnswerIsUngraded_MarksSubmittedAndLeavesDegreeUntouched()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var written = CreateWrittenQuestion(id: 2);
        var quiz = CreateQuiz([(mcq, 5m), (written, 10m)]);

        var mcqAnswer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 }; // correct, worth 5
        var writtenAnswer = new AttemptAnswer { QuestionId = 2, TextAnswer = "some answer", Score = null }; // not graded yet
        var attempt = CreateAttempt([mcqAnswer, writtenAnswer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
        Assert.Equal(0m, attempt.Degree); // NOT set to 5 — Degree stays untouched while pending
        Assert.True(mcqAnswer.IsCorrect); // MCQ grading still happened even though attempt is Submitted
    }

    [Fact]
    public async Task FinalizeAttempt_WhenWrittenAnswerIsMissingEntirely_MarksSubmitted()
    {
        // Arrange
        var written = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz([(written, 10m)]);
        var attempt = CreateAttempt(); // no answers submitted at all

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
    }

    [Fact]
    public async Task FinalizeAttempt_WhenWrittenAnswerIsAlreadyGraded_CountsItsScoreTowardTotal()
    {
        // Arrange - simulates a written answer that was pre-graded before finalize runs
        var written = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz([(written, 10m)]);
        var writtenAnswer = new AttemptAnswer { QuestionId = 1, TextAnswer = "answer", Score = 7m };
        var attempt = CreateAttempt([writtenAnswer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Graded, attempt.Status);
        Assert.Equal(7m, attempt.Degree);
    }

    #endregion

    #region FinalizeAttempt - Security violations

    [Fact]
    public async Task FinalizeAttempt_WhenSecurityViolationAndNoPendingWritten_MarksSubmittedWithComputedScore()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)]);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer], tabSwitchCount: 2);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status); // held for review, not Graded
        Assert.Equal(5m, attempt.Degree); // but score IS computed and saved this time
    }

    [Fact]
    public async Task FinalizeAttempt_WhenCopyPasteAttemptDetected_TriggersSecurityHold()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)]);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer], copyPasteCount: 1);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
    }

    [Fact]
    public async Task FinalizeAttempt_WhenPendingWrittenAndSecurityViolationBothPresent_PendingWrittenTakesPriority()
    {
        // Arrange - pending written check happens before the security check in the if/else chain,
        // so Degree should remain untouched here too, not computed like the pure-security-violation case
        var written = CreateWrittenQuestion(id: 1);
        var quiz = CreateQuiz([(written, 10m)]);
        var writtenAnswer = new AttemptAnswer { QuestionId = 1, Score = null };
        var attempt = CreateAttempt([writtenAnswer], tabSwitchCount: 3);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
        Assert.Equal(0m, attempt.Degree); // Degree untouched, same as the plain-pending case
    }

    #endregion

    #region FinalizeAttempt - Enrollment completion

    [Fact]
    public async Task FinalizeAttempt_WhenFullyGradedAndLessonQuizScope_MarksEnrollmentCompleted()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)], scope: QuizScope.LessonQuiz, lessonId: 20);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);

        var enrollment = new Enrollment { StudentId = StudentId, LessonId = 20, IsCompleted = false };
        _enrollmentRepository
            .FirstOrDefaultAsync(Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(enrollment);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.True(enrollment.IsCompleted);
        Assert.NotNull(enrollment.CompletedAt);
    }

    [Fact]
    public async Task FinalizeAttempt_WhenComprehensiveExamScope_DoesNotQueryEnrollmentRepository()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)], scope: QuizScope.ComprehensiveExam);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        await _enrollmentRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeAttempt_WhenSubmittedNotGraded_DoesNotMarkEnrollmentCompleted()
    {
        // Arrange - security violation keeps it as Submitted, not Graded
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)], scope: QuizScope.LessonQuiz, lessonId: 20);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer], tabSwitchCount: 1);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        await _enrollmentRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeAttempt_WhenEnrollmentAlreadyCompleted_DoesNotOverwriteCompletedAt()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)], scope: QuizScope.LessonQuiz, lessonId: 20);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);

        var originalCompletedAt = DateTimeOffset.UtcNow.AddDays(-3);
        var enrollment = new Enrollment
        {
            StudentId = StudentId,
            LessonId = 20,
            IsCompleted = true,
            CompletedAt = originalCompletedAt
        };
        _enrollmentRepository
            .FirstOrDefaultAsync(Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(enrollment);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(originalCompletedAt, enrollment.CompletedAt);
    }

    [Fact]
    public async Task FinalizeAttempt_WhenEnrollmentNotFound_DoesNotThrow()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)], scope: QuizScope.LessonQuiz, lessonId: 20);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);
        // default setup already returns null for enrollment lookup

        // Act & Assert (should not throw)
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);
        Assert.Equal(QuizAttemptStatus.Graded, attempt.Status);
    }

    #endregion

    #region FinalizeAttempt - side effects

    [Fact]
    public async Task FinalizeAttempt_AlwaysSetsSubmittedAtAndCallsSaveChangesOnce()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1, correctChoiceId: 100);
        var quiz = CreateQuiz([(mcq, 5m)]);
        var answer = new AttemptAnswer { QuestionId = 1, ChoiceId = 100 };
        var attempt = CreateAttempt([answer]);

        // Act
        await QuizFinalizer.FinalizeAttempt(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.NotNull(attempt.SubmittedAt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region FinalizeAfterManualGrading

    [Fact]
    public async Task FinalizeAfterManualGrading_WhenAnyAnswerStillUngraded_ReturnsWithoutChanges()
    {
        // Arrange
        var quiz = CreateQuiz([(CreateWrittenQuestion(1), 10m), (CreateWrittenQuestion(2), 10m)],
            scope: QuizScope.LessonQuiz, lessonId: 20);

        var attempt = CreateAttempt(
        [
            new AttemptAnswer { QuestionId = 1, Score = 8m },
            new AttemptAnswer { QuestionId = 2, Score = null } // still pending
        ]);
        attempt.Status = QuizAttemptStatus.Submitted;

        // Act
        await QuizFinalizer.FinalizeAfterManualGrading(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status); // unchanged
        Assert.Equal(0m, attempt.Degree); // never touched

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeAfterManualGrading_WhenAllAnswersGradedAndNoSecurityViolation_MarksGradedAndSumsScores()
    {
        // Arrange
        var quiz = CreateQuiz([(CreateWrittenQuestion(1), 10m), (CreateWrittenQuestion(2), 10m)],
            scope: QuizScope.ComprehensiveExam);

        var attempt = CreateAttempt(
        [
            new AttemptAnswer { QuestionId = 1, Score = 8m },
            new AttemptAnswer { QuestionId = 2, Score = 6m }
        ]);
        attempt.Status = QuizAttemptStatus.Submitted;

        // Act
        await QuizFinalizer.FinalizeAfterManualGrading(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Graded, attempt.Status);
        Assert.Equal(14m, attempt.Degree);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeAfterManualGrading_WhenAllGradedButSecurityViolationExists_KeepsSubmittedStatus()
    {
        // Arrange
        var quiz = CreateQuiz([(CreateWrittenQuestion(1), 10m)], scope: QuizScope.LessonQuiz, lessonId: 20);
        var attempt = CreateAttempt(
            [new AttemptAnswer { QuestionId = 1, Score = 9m }],
            tabSwitchCount: 1);
        attempt.Status = QuizAttemptStatus.Submitted;

        // Act
        await QuizFinalizer.FinalizeAfterManualGrading(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status); // stays Submitted despite full grading
        Assert.Equal(9m, attempt.Degree); // Degree IS summed regardless of security status

        await _enrollmentRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FinalizeAfterManualGrading_WhenGradedAndLessonQuizScope_MarksEnrollmentCompleted()
    {
        // Arrange
        var quiz = CreateQuiz([(CreateWrittenQuestion(1), 10m)], scope: QuizScope.LessonQuiz, lessonId: 20);
        var attempt = CreateAttempt([new AttemptAnswer { QuestionId = 1, Score = 10m }]);
        attempt.Status = QuizAttemptStatus.Submitted;

        var enrollment = new Enrollment { StudentId = StudentId, LessonId = 20, IsCompleted = false };
        _enrollmentRepository
            .FirstOrDefaultAsync(Arg.Any<StudentLessonEnrollmentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(enrollment);

        // Act
        await QuizFinalizer.FinalizeAfterManualGrading(attempt, quiz, _unitOfWork, CancellationToken.None);

        // Assert
        Assert.True(enrollment.IsCompleted);
    }

    #endregion
}
