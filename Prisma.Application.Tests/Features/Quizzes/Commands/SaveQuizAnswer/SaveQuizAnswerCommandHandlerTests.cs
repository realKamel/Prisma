
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.SaveQuizAnswer;

public class SaveQuizAnswerCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<AttemptAnswer, int> _answerRepository = Substitute.For<IRepository<AttemptAnswer, int>>();
    private readonly SaveQuizAnswerCommandHandler _handler;

    private static readonly Guid StudentId = Guid.NewGuid();

    public SaveQuizAnswerCommandHandlerTests()
    {
        _currentUser.UserId.Returns(StudentId);

        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<AttemptAnswer, int>().Returns(_answerRepository);

        _handler = new SaveQuizAnswerCommandHandler(_unitOfWork, _currentUser);
    }

    #region Helpers

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

    private static Quiz CreateQuiz(int id = 10, int durationMinutes = 30) =>
        new() { Id = id, TimeInMinutes = TimeSpan.FromMinutes(durationMinutes) };

    private void SetupAttempt(QuizAttempt? attempt) =>
        _attemptRepository
            .FirstOrDefaultAsync(Arg.Any<AttemptByIdAndStudentSpecification>(), Arg.Any<CancellationToken>())
            .Returns(attempt);

    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    private static SaveQuizAnswerCommand McqAnswerCommand(int attemptId = 1, int questionId = 1, int choiceId = 100) =>
        new(attemptId, questionId, choiceId, null);

    private static SaveQuizAnswerCommand WrittenAnswerCommand(int attemptId = 1, int questionId = 1, string text = "my answer") =>
        new(attemptId, questionId, null, text);

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenAttemptNotFound_ReturnsFailure()
    {
        // Arrange
        SetupAttempt(null);

        // Act
        var result = await _handler.Handle(McqAnswerCommand(), CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("المحاولة غير موجودة", result.Message);

        await _quizRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<QuizByIdSpecification>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(QuizAttemptStatus.Submitted)]
    [InlineData(QuizAttemptStatus.Graded)]
    public async Task Handle_WhenAttemptNotInProgress_ReturnsFailure(QuizAttemptStatus status)
    {
        // Arrange
        SetupAttempt(CreateAttempt(status: status));

        // Act
        var result = await _handler.Handle(McqAnswerCommand(), CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("لا يمكن تعديل الإجابات بعد التسليم", result.Message);

        await _quizRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<QuizByIdSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTimeExpired_ReturnsFailureWithoutSavingAnswer()
    {
        // Arrange - 30 min quiz started 40 minutes ago, well past the 5s grace period
        var attempt = CreateAttempt(startedAt: DateTimeOffset.UtcNow.AddMinutes(-40));
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz(durationMinutes: 30));

        // Act
        var result = await _handler.Handle(McqAnswerCommand(), CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("انتهى وقت الاختبار، لا يمكن حفظ المزيد من الإجابات", result.Message);

        _answerRepository.DidNotReceive().Add(Arg.Any<AttemptAnswer>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenWithinGracePeriodAfterDeadline_StillSavesSuccessfully()
    {
        // Arrange - 30 min quiz started exactly 30 min + 2s ago, within the 5s grace window
        var attempt = CreateAttempt(startedAt: DateTimeOffset.UtcNow.AddMinutes(-30).AddSeconds(-2));
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz(durationMinutes: 30));

        // Act
        var result = await _handler.Handle(McqAnswerCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
    }

    #endregion

    #region New answer creation

    [Fact]
    public async Task Handle_WhenNoExistingAnswerForQuestion_CreatesNewAnswer()
    {
        // Arrange
        var attempt = CreateAttempt();
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz());

        var command = McqAnswerCommand(questionId: 5, choiceId: 100);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("تم حفظ الإجابة", result.Message);

        _answerRepository.Received(1).Add(Arg.Is<AttemptAnswer>(a =>
            a.QuizAttemptId == attempt.Id &&
            a.StudentId == StudentId &&
            a.QuestionId == 5 &&
            a.ChoiceId == 100 &&
            a.TextAnswer == null));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSavingWrittenAnswer_CreatesAnswerWithTextAndNullChoice()
    {
        // Arrange
        var attempt = CreateAttempt();
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz());

        var command = WrittenAnswerCommand(questionId: 5, text: "my written answer");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _answerRepository.Received(1).Add(Arg.Is<AttemptAnswer>(a =>
            a.QuestionId == 5 &&
            a.ChoiceId == null &&
            a.TextAnswer == "my written answer"));
    }

    #endregion

    #region Updating existing answer

    [Fact]
    public async Task Handle_WhenAnswerAlreadyExistsForQuestion_UpdatesInPlaceWithoutAddingNew()
    {
        // Arrange
        var existingAnswer = new AttemptAnswer { QuestionId = 5, ChoiceId = 100, TextAnswer = null };
        var attempt = CreateAttempt(answers: [existingAnswer]);
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz());

        var command = McqAnswerCommand(questionId: 5, choiceId: 200); // student changed their choice

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(200, existingAnswer.ChoiceId);

        _answerRepository.DidNotReceive().Add(Arg.Any<AttemptAnswer>());
    }

    [Fact]
    public async Task Handle_WhenSwitchingFromChoiceToTextAnswer_OverwritesBothFieldsOnExisting()
    {
        // Arrange - existing answer had a choice, new submission replaces it with text
        var existingAnswer = new AttemptAnswer { QuestionId = 5, ChoiceId = 100, TextAnswer = null };
        var attempt = CreateAttempt(answers: [existingAnswer]);
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz());

        var command = new SaveQuizAnswerCommand(attempt.Id, 5, null, "changed my mind, here's text");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(existingAnswer.ChoiceId);
        Assert.Equal("changed my mind, here's text", existingAnswer.TextAnswer);
    }

    [Fact]
    public async Task Handle_WhenMultipleQuestionsAnswered_OnlyMatchingQuestionIsUpdated()
    {
        // Arrange
        var answerForQ1 = new AttemptAnswer { QuestionId = 1, ChoiceId = 10 };
        var answerForQ2 = new AttemptAnswer { QuestionId = 2, ChoiceId = 20 };
        var attempt = CreateAttempt(answers: [answerForQ1, answerForQ2]);
        SetupAttempt(attempt);
        SetupQuiz(CreateQuiz());

        var command = McqAnswerCommand(questionId: 2, choiceId: 99);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(10, answerForQ1.ChoiceId); // untouched
        Assert.Equal(99, answerForQ2.ChoiceId); // updated
    }

    #endregion
}
