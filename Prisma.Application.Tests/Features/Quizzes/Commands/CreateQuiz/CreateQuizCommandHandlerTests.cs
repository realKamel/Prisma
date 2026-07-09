
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Commands.CreateQuiz;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Tests.Features.Quizzes.Commands.CreateQuiz;


public class CreateQuizCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly IRepository<Lesson, int> _lessonRepository = Substitute.For<IRepository<Lesson, int>>();
    private readonly IRepository<Question, int> _questionRepository = Substitute.For<IRepository<Question, int>>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<QuestionLessonQuiz, int> _linkRepository =
        Substitute.For<IRepository<QuestionLessonQuiz, int>>();
    private readonly CreateQuizCommandHandler _handler;

    private int _questionIdCounter = 1;
    private int _quizIdCounter = 1;

    public CreateQuizCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepository);
        _unitOfWork.GetOrCreateRepository<Question, int>().Returns(_questionRepository);
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<QuestionLessonQuiz, int>().Returns(_linkRepository);

        // simulate EF-generated Ids on Add, since the handler relies on them being
        // populated before SaveChangesAsync is called again for the links
        _questionRepository.When(x => x.Add(Arg.Any<Question>()))
            .Do(ci => ci.Arg<Question>().Id = _questionIdCounter++);

        _quizRepository.When(x => x.Add(Arg.Any<Quiz>()))
            .Do(ci => ci.Arg<Quiz>().Id = _quizIdCounter++);

        _handler = new CreateQuizCommandHandler(_unitOfWork, _currentUser);
    }

    #region Helpers

    private static CreateQuizChoiceDto Choice(string text, bool isCorrect) =>
        new() { Text = text, IsCorrect = isCorrect };

    private static CreateQuizQuestionDto McqQuestion(string text = "What is 2+2?", decimal degree = 5m) =>
        new()
        {
            Text = text,
            Type = QuestionType.MCQ,
            Degree = degree,
            Choices = new List<CreateQuizChoiceDto>
            {
                Choice("3", false),
                Choice("4", true),
                Choice("5", false)
            }
        };

    private static CreateQuizQuestionDto WrittenQuestionDto(
        string text = "Explain X", decimal degree = 10m, string modelAnswer = "Because Y") =>
        new()
        {
            Text = text,
            Type = QuestionType.Written,
            Degree = degree,
            ModelAnswer = modelAnswer
        };

    private static CreateQuizCommand CreateCommand(
        QuizScope scope = QuizScope.LessonQuiz,
        int? lessonId = 10,
        int? academicYearId = null,
        List<CreateQuizQuestionDto>? questions = null,
        DateTimeOffset? availableFrom = null,
        DateTimeOffset? dueDate = null) =>
        new(
            Title: "Quiz 1",
            Description: "Description",
            Scope: scope,
            LessonId: lessonId,
            AcademicYearId: academicYearId,
            DurationMinutes: 30,
            AvailableFrom: availableFrom,
            DueDate: dueDate,
            Questions: questions ?? new List<CreateQuizQuestionDto> { McqQuestion() }
        );

    private static Lesson CreateLesson(int id, int? quizId = null) =>
        new() { Id = id, Title = "Lesson", QuizId = quizId };

    #endregion

    #region Guards

    [Fact]
    public async Task Handle_WhenLessonQuizScopeAndLessonNotFound_ReturnsFailure()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.LessonQuiz, lessonId: 10);

        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("الحصة غير موجودة", result.Message);

        _questionRepository.DidNotReceive().Add(Arg.Any<Question>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonQuizScopeAndLessonAlreadyHasQuiz_ReturnsFailure()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.LessonQuiz, lessonId: 10);
        var lesson = CreateLesson(id: 10, quizId: 99);

        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("الحصة دي عندها اختبار بالفعل", result.Message);

        _questionRepository.DidNotReceive().Add(Arg.Any<Question>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenComprehensiveExamScope_DoesNotQueryLessonRepository()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.ComprehensiveExam, lessonId: null, academicYearId: 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        await _lessonRepository.DidNotReceive().FirstOrDefaultAsync(
            Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Question building

    [Fact]
    public async Task Handle_WhenQuestionIsWritten_CreatesWrittenQuestionWithModelAnswerAndNoChoices()
    {
        // Arrange
        var command = CreateCommand(
            scope: QuizScope.ComprehensiveExam,
            lessonId: null,
            academicYearId: 5,
            questions: new List<CreateQuizQuestionDto> { WrittenQuestionDto(modelAnswer: "42") });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _questionRepository.Received(1).Add(Arg.Is<Question>(q =>
            q is WrittenQuestion &&
            ((WrittenQuestion)q).Answer == "42" &&
            q.Title == "Explain X" &&
            q.Type == QuestionType.Written));
    }

    [Fact]
    public async Task Handle_WhenQuestionIsMcq_CreatesMcqQuestionWithMappedChoices()
    {
        // Arrange
        var command = CreateCommand(
            scope: QuizScope.ComprehensiveExam,
            lessonId: null,
            academicYearId: 5,
            questions: new List<CreateQuizQuestionDto> { McqQuestion() });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _questionRepository.Received(1).Add(Arg.Is<Question>(q =>
            q is MCQQuestion &&
            ((MCQQuestion)q).Choices.Count == 3 &&
            ((MCQQuestion)q).Choices.Count(c => c.IsCorrect) == 1 &&
            ((MCQQuestion)q).Choices.Single(c => c.IsCorrect).Text == "4"));
    }

    [Fact]
    public async Task Handle_WhenMultipleQuestions_SumsTotalDegreeCorrectly()
    {
        // Arrange
        var command = CreateCommand(
            scope: QuizScope.ComprehensiveExam,
            lessonId: null,
            academicYearId: 5,
            questions: new List<CreateQuizQuestionDto>
            {
                McqQuestion(degree: 5m),
                WrittenQuestionDto(degree: 10m),
                McqQuestion(degree: 7.5m)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(22.5m, result.Data!.TotalDegree);
        Assert.Equal(3, result.Data.QuestionsCount);
    }

    #endregion

    #region Quiz building

    [Fact]
    public async Task Handle_WhenLessonQuizScope_SetsLessonIdAndClearsAcademicYearId()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.LessonQuiz, lessonId: 10, academicYearId: 99);
        var lesson = CreateLesson(id: 10);

        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _quizRepository.Received(1).Add(Arg.Is<Quiz>(q =>
            q.LessonId == 10 && q.AcademicYearId == null));
    }

    [Fact]
    public async Task Handle_WhenComprehensiveExamScope_SetsAcademicYearIdAndClearsLessonId()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.ComprehensiveExam, lessonId: null, academicYearId: 5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _quizRepository.Received(1).Add(Arg.Is<Quiz>(q =>
            q.AcademicYearId == 5 && q.LessonId == null));
    }

    [Fact]
    public async Task Handle_MapsDurationMinutesToTimeSpanCorrectly()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.ComprehensiveExam, lessonId: null, academicYearId: 5)
            with
        { DurationMinutes = 45 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _quizRepository.Received(1).Add(Arg.Is<Quiz>(q => q.TimeInMinutes == TimeSpan.FromMinutes(45)));
    }

    #endregion

    #region Linking

    [Fact]
    public async Task Handle_CreatesOneLinkPerQuestionWithCorrectDegreeAndIds()
    {
        // Arrange
        var command = CreateCommand(
            scope: QuizScope.ComprehensiveExam,
            lessonId: null,
            academicYearId: 5,
            questions: new List<CreateQuizQuestionDto>
            {
                McqQuestion(degree: 5m),
                WrittenQuestionDto(degree: 10m)
            });

        List<QuestionLessonQuiz>? capturedLinks = null;
        _linkRepository
            .When(x => x.AddRange(Arg.Any<IEnumerable<QuestionLessonQuiz>>()))
            .Do(ci => capturedLinks = ci.Arg<IEnumerable<QuestionLessonQuiz>>().ToList());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLinks);
        Assert.Equal(2, capturedLinks!.Count);

        Assert.All(capturedLinks, link => Assert.Equal(result.Data!.QuizId, link.LessonQuizId));
        Assert.Contains(capturedLinks, l => l.Degree == 5m);
        Assert.Contains(capturedLinks, l => l.Degree == 10m);

        // question ids must be the real generated ids, not zero
        Assert.All(capturedLinks, l => Assert.NotEqual(0, l.QuestionId));
    }

    #endregion

    #region Lesson update

    [Fact]
    public async Task Handle_WhenLessonQuizScope_LinksLessonToNewQuiz()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.LessonQuiz, lessonId: 10);
        var lesson = CreateLesson(id: 10);

        _lessonRepository
            .FirstOrDefaultAsync(Arg.Any<LessonByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(lesson);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(result.Data!.QuizId, lesson.QuizId);
    }

    #endregion

    #region Return value & SaveChanges

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsExpectedDtoDefaults()
    {
        // Arrange
        var availableFrom = DateTimeOffset.UtcNow;
        var dueDate = availableFrom.AddDays(7);
        var command = CreateCommand(
            scope: QuizScope.ComprehensiveExam,
            lessonId: null,
            academicYearId: 5,
            availableFrom: availableFrom,
            dueDate: dueDate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("تم إنشاء الاختبار بنجاح", result.Message);

        var dto = result.Data!;
        Assert.Equal("Quiz 1", dto.Title);
        Assert.Equal("Description", dto.Description);
        Assert.Equal(30, dto.DurationMinutes);
        Assert.Equal(availableFrom, dto.AvailableFrom);
        Assert.Equal(dueDate, dto.DueDate);
        Assert.Equal(0, dto.SubmittedCount);
        Assert.Equal(0, dto.PendingGradingCount);
        Assert.Null(dto.AverageScore);
        Assert.Equal("active", dto.Status);
        Assert.NotEqual(0, dto.QuizId);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_CallsSaveChangesExactlyThreeTimes()
    {
        // Arrange
        var command = CreateCommand(scope: QuizScope.ComprehensiveExam, lessonId: null, academicYearId: 5);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(3).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion
}
