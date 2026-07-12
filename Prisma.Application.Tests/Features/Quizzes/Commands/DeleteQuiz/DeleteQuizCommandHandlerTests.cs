
using NSubstitute;
using Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.DeleteQuiz;


public class DeleteQuizCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<Lesson, int> _lessonRepository = Substitute.For<IRepository<Lesson, int>>();
    private readonly DeleteQuizCommandHandler _handler;

    private static readonly DeleteQuizCommand ValidCommand = new(QuizId: 1);

    public DeleteQuizCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepository);

        _handler = new DeleteQuizCommandHandler(_unitOfWork);
    }

    #region Helpers

    private static Quiz CreateQuiz(
        int id = 1,
        QuizScope scope = QuizScope.ComprehensiveExam,
        int? lessonId = null,
        IEnumerable<QuizAttempt>? attempts = null) =>
        new()
        {
            Id = id,
            Title = "Quiz",
            Scope = scope,
            LessonId = lessonId,
            Attempts = attempts?.ToList() ?? new List<QuizAttempt>()
        };

    private static QuizAttempt CreateAttempt(QuizAttemptStatus status) =>
        new() { Status = status, StartedAt = DateTimeOffset.UtcNow };

    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizByIdForDeleteSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenQuizNotFound_ReturnsFailure()
    {
        // Arrange
        SetupQuiz(null);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("الاختبار غير موجود", result.Message);

        _quizRepository.DidNotReceive().Update(Arg.Any<Quiz>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuizHasSubmittedAttempt_ReturnsFailureAndDoesNotDelete()
    {
        // Arrange
        var quiz = CreateQuiz(attempts: [CreateAttempt(QuizAttemptStatus.Submitted)]);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("مينفعش تحذف/ي اختبار عنده محاولات مسلمة أو متصححة", result.Message);
        Assert.False(quiz.IsDeleted);

        _quizRepository.DidNotReceive().Update(Arg.Any<Quiz>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuizHasGradedAttempt_ReturnsFailureAndDoesNotDelete()
    {
        // Arrange
        var quiz = CreateQuiz(attempts: [CreateAttempt(QuizAttemptStatus.Graded)]);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.False(quiz.IsDeleted);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOneAttemptIsInProgressAndAnotherIsSubmitted_ReturnsFailure()
    {
        // Arrange - a single non-InProgress attempt is enough to block deletion
        var quiz = CreateQuiz(attempts:
        [
            CreateAttempt(QuizAttemptStatus.InProgress),
            CreateAttempt(QuizAttemptStatus.Submitted)
        ]);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
    }

    #endregion

    #region Successful deletion

    [Fact]
    public async Task Handle_WhenQuizHasNoAttempts_SoftDeletesSuccessfully()
    {
        // Arrange
        var quiz = CreateQuiz();
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("تم حذف الاختبار بنجاح", result.Message);
        Assert.True(quiz.IsDeleted);
        Assert.NotNull(quiz.DeletedAt);

        _quizRepository.Received(1).Update(quiz);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllAttemptsAreInProgress_SoftDeletesSuccessfully()
    {
        // Arrange
        var quiz = CreateQuiz(attempts:
        [
            CreateAttempt(QuizAttemptStatus.InProgress),
            CreateAttempt(QuizAttemptStatus.InProgress)
        ]);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(quiz.IsDeleted);
    }

    #endregion

    #region Lesson unlinking

    [Fact]
    public async Task Handle_WhenLessonQuizScopeAndLessonExists_UnlinksLessonFromQuiz()
    {
        // Arrange
        var quiz = CreateQuiz(scope: QuizScope.LessonQuiz, lessonId: 10);
        var lesson = new Lesson { Id = 10, Title = "Lesson", QuizId = quiz.Id };

        SetupQuiz(quiz);
        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Null(lesson.QuizId);
    }

    [Fact]
    public async Task Handle_WhenLessonQuizScopeButLessonNotFound_StillDeletesQuizSuccessfully()
    {
        // Arrange
        var quiz = CreateQuiz(scope: QuizScope.LessonQuiz, lessonId: 10);

        SetupQuiz(quiz);
        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(quiz.IsDeleted);
    }

    [Fact]
    public async Task Handle_WhenComprehensiveExamScope_DoesNotQueryLessonRepository()
    {
        // Arrange
        var quiz = CreateQuiz(scope: QuizScope.ComprehensiveExam, lessonId: null);
        SetupQuiz(quiz);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        await _lessonRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion
}