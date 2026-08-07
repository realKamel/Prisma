
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
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly DeleteQuizCommandHandler _handler;

    private static readonly DeleteQuizCommand ValidCommand = new(QuizId: 1);

    public DeleteQuizCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepository);
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);

        _handler = new DeleteQuizCommandHandler(_unitOfWork);
    }

    #region Helpers

    private static Quiz CreateQuiz(
        int id = 1,
        QuizScope scope = QuizScope.ComprehensiveExam,
        int? lessonId = null) =>
        new()
        {
            Id = id,
            Title = "Quiz",
            Scope = scope,
            LessonId = lessonId,
        };


    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    private void SetupHasSubmittedAttempts(bool hasAttempts)
    {
        _attemptRepository
            .AnyAsync(
                Arg.Any<SubmittedAttemptsForQuizSpecification>(),
                Arg.Any<CancellationToken>())
            .Returns(hasAttempts);
    }

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
        Assert.False(result.IsSuccess);
        Assert.Equal("الاختبار غير موجود", result.GetResultMessage());

        _quizRepository.DidNotReceive().Update(Arg.Any<Quiz>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuizHasSubmittedAttempt_ReturnsFailureAndDoesNotDelete()
    {
        // Arrange
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupHasSubmittedAttempts(true);


        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("مينفعش تحذف/ي اختبار عنده محاولات مسلمة أو متصححة", result.GetResultMessage());
        Assert.False(quiz.IsDeleted);

        _quizRepository.DidNotReceive().Update(Arg.Any<Quiz>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Successful deletion

    [Fact]
    public async Task Handle_WhenQuizHasNoSubmittedAttempts_SoftDeletesSuccessfully()
    {
        // Arrange
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupHasSubmittedAttempts(false);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("تم حذف الاختبار بنجاح", result.GetResultMessage());
        Assert.True(quiz.IsDeleted);
        Assert.NotNull(quiz.DeletedAt);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAllAttemptsAreInProgress_SoftDeletesSuccessfully()
    {
        // Arrange
        var quiz = CreateQuiz();

        SetupQuiz(quiz);

        // InProgress attempts are ignored by the specification
        SetupHasSubmittedAttempts(false);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
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
        SetupHasSubmittedAttempts(false);

        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        // Act
        var result = await _handler.Handle(ValidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
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
        Assert.True(result.IsSuccess);
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
        Assert.True(result.IsSuccess);
        await _lessonRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion
}