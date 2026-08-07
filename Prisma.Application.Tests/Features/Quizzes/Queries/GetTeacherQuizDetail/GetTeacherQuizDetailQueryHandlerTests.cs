
using NSubstitute;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizDetail;
using Prisma.Application.Features.Quizzes.Specifications;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetTeacherQuizDetail;


public class GetTeacherQuizDetailQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<QuestionLessonQuiz, int> _questionLessonRepository =
    Substitute.For<IRepository<QuestionLessonQuiz, int>>();
    private readonly GetTeacherQuizDetailQueryHandler _handler;

    private static readonly GetTeacherQuizDetailQuery ValidQuery = new(QuizId: 1);

    public GetTeacherQuizDetailQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<QuestionLessonQuiz, int>().Returns(_questionLessonRepository);
        _handler = new GetTeacherQuizDetailQueryHandler(_unitOfWork);
    }

    #region Helpers

    private static MCQQuestion CreateMcqQuestion(int id) =>
        new()
        {
            Id = id,
            Title = "2+2?",
            Type = QuestionType.MCQ,
            Choices = new List<Choice>
            {
                new() { Id = 100, Text = "4", IsCorrect = true },
                new() { Id = 101, Text = "5", IsCorrect = false }
            }
        };

    private static WrittenQuestion CreateWrittenQuestion(int id, string? modelAnswer = "42") =>
        new() { Id = id, Title = "Explain X", Type = QuestionType.Written, Answer = modelAnswer };

    private static QuizAttempt CreateAttempt(QuizAttemptStatus status, decimal degree = 0m) =>
        new() { Status = status, Degree = degree, StartedAt = DateTimeOffset.UtcNow.AddDays(-1) };

    private static TeacherQuizDetailProjection CreateQuiz(
    int id = 1,
    string title = "Quiz",
    decimal totalDegree = 100m,
    int durationMinutes = 30,
    QuizScope scope = QuizScope.ComprehensiveExam,
    int? lessonId = null,
    string? lessonTitle = null,
    int? academicYearId = null,
    string? academicYearName = null,
    int submittedCount = 0,
    int pendingGradingCount = 0,
    decimal? averageDegree = null,
    bool hasAttempts = false,
    bool hasUngradedAttempts = false)
    => new()
    {
        QuizId = id,
        Title = title,
        Scope = scope,
        LessonId = lessonId,
        LessonTitle = lessonTitle,
        AcademicYearId = academicYearId,
        AcademicYearName = academicYearName,
        TimeInMinutes = TimeSpan.FromMinutes(durationMinutes),
        TotalDegree = totalDegree,

        SubmittedCount = submittedCount,
        PendingGradingCount = pendingGradingCount,
        AverageDegree = averageDegree,
        HasAttempts = hasAttempts,
        HasUngradedAttempts = hasUngradedAttempts
    };

    private void SetupQuiz(TeacherQuizDetailProjection? quiz) =>
    _quizRepository
        .FirstOrDefaultAsync(
            Arg.Any<TeacherQuizDetailSpecification>(),
            Arg.Any<CancellationToken>())
        .Returns(quiz);

    private void SetupQuestions(params QuestionLessonQuiz[] questions) =>
    _questionLessonRepository
        .ListAsync(
            Arg.Any<TeacherQuizQuestionsSpecification>(),
            Arg.Any<CancellationToken>())
        .Returns(questions.ToList());
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

    #endregion

    #region Stats & status (spot-checks — full matrix covered in GetTeacherQuizzesListQueryHandlerTests)

    [Fact]
    public async Task Handle_WhenAtLeastOneSubmittedAttempt_ReturnsPendingGradingStatus()
    {
        // Arrange
        var quiz = CreateQuiz(
            submittedCount: 2,
            pendingGradingCount: 1,
            hasAttempts: true,
            hasUngradedAttempts: true);

        SetupQuiz(quiz);
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("pending_grading", result.Value!.Status);
        Assert.Equal(1, result.Value.PendingGradingCount);
        Assert.Equal(2, result.Value.SubmittedCount);
    }
    [Fact]
    public async Task Handle_WhenAllAttemptsGraded_ReturnsCompletedStatusWithAverageScore()
    {
        // Arrange
        var quiz = CreateQuiz(
            totalDegree: 100m,
            submittedCount: 2,
            pendingGradingCount: 0,
            averageDegree: 70m,
            hasAttempts: true,
            hasUngradedAttempts: false);

        SetupQuiz(quiz);
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("completed", result.Value!.Status);
        Assert.Equal(70.0, result.Value.AverageScore);
    }

    [Fact]
    public async Task Handle_WhenNoAttempts_ReturnsActiveStatusWithNullAverageScore()
    {
        // Arrange
        SetupQuiz(CreateQuiz());
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("active", result.Value!.Status);
        Assert.Null(result.Value.AverageScore);
    }

    [Fact]
    public async Task Handle_WhenTotalDegreeIsZero_ReturnsNullAverageScoreEvenWithGradedAttempts()
    {
        // Arrange
        var quiz = CreateQuiz(
            totalDegree: 0m,
            submittedCount: 1,
            pendingGradingCount: 0,
            averageDegree: null,
            hasAttempts: true,
            hasUngradedAttempts: false);

        SetupQuiz(quiz);
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Null(result.Value!.AverageScore);
    }

    #endregion

    #region Questions mapping - MCQ

    [Fact]
    public async Task Handle_WhenQuestionIsMcq_MapsChoicesIncludingIsCorrectFlag()
    {
        // Arrange
        var question = CreateMcqQuestion(id: 1);

        SetupQuiz(CreateQuiz());

        SetupQuestions(
            new QuestionLessonQuiz
            {
                QuestionId = 1,
                Question = question,
                Degree = 5m
            });

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!.Questions);
        Assert.Equal(2, dto.Choices!.Count);
        Assert.True(dto.Choices.Single(c => c.Text == "4").IsCorrect);
        Assert.False(dto.Choices.Single(c => c.Text == "5").IsCorrect);
        Assert.Null(dto.ModelAnswer);
    }
    #endregion

    #region Questions mapping - Written

    [Fact]
    public async Task Handle_WhenQuestionIsWritten_MapsModelAnswerWithNullChoices()
    {
        // Arrange
        var question = CreateWrittenQuestion(id: 1, modelAnswer: "The answer is 42");

        SetupQuiz(CreateQuiz());

        SetupQuestions(
            new QuestionLessonQuiz
            {
                QuestionId = 1,
                Question = question,
                Degree = 10m
            });

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Value!.Questions);
        Assert.Null(dto.Choices);
        Assert.Equal("The answer is 42", dto.ModelAnswer);
    }
    #endregion

    #region Questions mapping - general

    [Fact]
    public async Task Handle_MapsQuestionDegreeFromLinkNotFromQuestionItself()
    {
        // Arrange - Degree lives on QuestionLessonQuiz, not on the Question entity
        var question = CreateWrittenQuestion(id: 1);
        SetupQuestions(
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = question, Degree = 15m }
        ]);
        SetupQuiz(CreateQuiz());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(15m, Assert.Single(result.Value!.Questions).Degree);
    }

    [Fact]
    public async Task Handle_WhenMultipleQuestions_PreservesAllInResult()
    {
        // Arrange
        var mcq = CreateMcqQuestion(id: 1);
        var written = CreateWrittenQuestion(id: 2);
        SetupQuestions(
        [
            new QuestionLessonQuiz { QuestionId = 1, Question = mcq, Degree = 5m },
            new QuestionLessonQuiz { QuestionId = 2, Question = written, Degree = 10m }
        ]);
        SetupQuiz(CreateQuiz());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Questions.Count);
    }

    #endregion

    #region Scope / Lesson / AcademicYear mapping

    [Fact]
    public async Task Handle_WhenLessonQuizScope_MapsScopeStringAndLessonTitle()
    {
        // Arrange
        var quiz = CreateQuiz(
            scope: QuizScope.LessonQuiz,
            lessonId: 10,
            lessonTitle: "Algebra Basics");

        SetupQuiz(quiz);
        SetupQuestions(); // no questions needed for this test

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("LessonQuiz", result.Value!.Scope);
        Assert.Equal(10, result.Value.LessonId);
        Assert.Equal("Algebra Basics", result.Value.LessonTitle);
        Assert.Null(result.Value.AcademicYearId);
        Assert.Null(result.Value.AcademicYearName);
    }

    [Fact]
    public async Task Handle_WhenComprehensiveExamScope_MapsScopeStringAndAcademicYearName()
    {
        // Arrange
        var quiz = CreateQuiz(
            scope: QuizScope.ComprehensiveExam,
            academicYearId: 5,
            academicYearName: "Grade 12");

        SetupQuiz(quiz);
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("ComprehensiveExam", result.Value!.Scope);
        Assert.Equal(5, result.Value.AcademicYearId);
        Assert.Equal("Grade 12", result.Value.AcademicYearName);
        Assert.Null(result.Value.LessonId);
        Assert.Null(result.Value.LessonTitle);
    }


    [Fact]
    public async Task Handle_WhenLessonNavigationNotLoaded_LessonTitleIsNullWithoutThrowing()
    {
        // Arrange
        var quiz = CreateQuiz(
            scope: QuizScope.LessonQuiz,
            lessonId: 10,
            lessonTitle: null);

        SetupQuiz(quiz);
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.Value!.LessonId);
        Assert.Null(result.Value.LessonTitle);
    }
    #endregion

    #region General mapping

    [Fact]
    public async Task Handle_MapsBasicQuizFieldsCorrectly()
    {
        // Arrange
        var quiz = CreateQuiz(
            id: 7,
            title: "Midterm",
            totalDegree: 50m,
            durationMinutes: 45);

        SetupQuiz(quiz);
        SetupQuestions();

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(7, result.Value!.QuizId);
        Assert.Equal("Midterm", result.Value.Title);
        Assert.Equal(50m, result.Value.TotalDegree);
        Assert.Equal(45, result.Value.DurationMinutes);
    }
    #endregion
}