using NSubstitute;
using Prisma.Application.Features.Quizzes.Queries.GetQuizStudents;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetQuizStudents;


public class GetQuizStudentsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Quiz, int> _quizRepository = Substitute.For<IRepository<Quiz, int>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepository = Substitute.For<IRepository<Enrollment, int>>();
    private readonly IRepository<Student, Guid> _studentRepository = Substitute.For<IRepository<Student, Guid>>();
    private readonly GetQuizStudentsQueryHandler _handler;

    private static readonly GetQuizStudentsQuery ValidQuery = new(QuizId: 1, Search: null, Status: null);

    public GetQuizStudentsQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Quiz, int>().Returns(_quizRepository);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepository);
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepository);

        _handler = new GetQuizStudentsQueryHandler(_unitOfWork);
    }

    #region Helpers

    private static Student CreateStudent(string firstName = "Sara", string lastName = "Ali") =>
        new() { Id = Guid.NewGuid(), FirstName = firstName, LastName = lastName };

    private static QuizAttempt CreateAttempt(
        Guid studentId,
        QuizAttemptStatus status,
        decimal degree = 0m,
        DateTimeOffset? submittedAt = null,
        int tabSwitchCount = 0,
        int copyPasteCount = 0,
        List<AttemptAnswer>? answers = null) =>
        new()
        {
            StudentId = studentId,
            Status = status,
            Degree = degree,
            SubmittedAt = submittedAt,
            TabSwitchCount = tabSwitchCount,
            CopyPasteAttemptCount = copyPasteCount,
            Answers = answers ?? new List<AttemptAnswer>()
        };

    private static Quiz CreateQuiz(
        int id = 1,
        string title = "Quiz",
        decimal totalDegree = 100m,
        QuizScope scope = QuizScope.ComprehensiveExam,
        int? lessonId = null,
        int? academicYearId = 5,
        DateTimeOffset? dueDate = null,
        List<QuizAttempt>? attempts = null) =>
        new()
        {
            Id = id,
            Title = title,
            TotalDegree = totalDegree,
            Scope = scope,
            LessonId = lessonId,
            AcademicYearId = academicYearId,
            DueDate = dueDate,
            Attempts = attempts ?? new List<QuizAttempt>()
        };

    private void SetupQuiz(Quiz? quiz) =>
        _quizRepository
            .FirstOrDefaultAsync(Arg.Any<QuizWithAttemptsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(quiz);

    private void SetupComprehensiveStudents(params Student[] students) =>
        _studentRepository
            .ListAsync(Arg.Any<StudentsByAcademicYearSpecification>(), Arg.Any<CancellationToken>())
            .Returns(students.ToList());

    private void SetupLessonEnrollments(params Student[] students) =>
        _enrollmentRepository
            .ListAsync(Arg.Any<EnrolledStudentsByLessonSpecification>(), Arg.Any<CancellationToken>())
            .Returns(students.Select(s => new Enrollment { Student = s, StudentId = s.Id }).ToList());

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
        Assert.False(result.Succeeded);
        Assert.Equal("الاختبار غير موجود", result.Message);
    }

    #endregion

    #region Student source based on scope

    [Fact]
    public async Task Handle_WhenLessonQuizScope_LoadsStudentsFromEnrollments()
    {
        // Arrange
        var student = CreateStudent();
        var quiz = CreateQuiz(scope: QuizScope.LessonQuiz, lessonId: 10, academicYearId: null);
        SetupQuiz(quiz);
        SetupLessonEnrollments(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Data!.TotalStudents);
        await _studentRepository.DidNotReceive().ListAsync(
            Arg.Any<StudentsByAcademicYearSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenLessonQuizScope_ExcludesEnrollmentsWithNullStudent()
    {
        // Arrange - defensive null-Student enrollment shouldn't crash or count
        var student = CreateStudent();
        var quiz = CreateQuiz(scope: QuizScope.LessonQuiz, lessonId: 10, academicYearId: null);
        SetupQuiz(quiz);

        _enrollmentRepository
            .ListAsync(Arg.Any<EnrolledStudentsByLessonSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<Enrollment>
            {
                new() { Student = student, StudentId = student.Id },
                new() { Student = null, StudentId = Guid.NewGuid() }
            });

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Data!.TotalStudents);
    }

    [Fact]
    public async Task Handle_WhenComprehensiveExamScope_LoadsStudentsFromAcademicYear()
    {
        // Arrange
        var student = CreateStudent();
        var quiz = CreateQuiz(scope: QuizScope.ComprehensiveExam, academicYearId: 5);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Data!.TotalStudents);
        await _enrollmentRepository.DidNotReceive().ListAsync(
            Arg.Any<EnrolledStudentsByLessonSpecification>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Status derivation - no attempt

    [Fact]
    public async Task Handle_WhenStudentHasNoAttemptAndDueDateNotPassed_ReturnsNotStarted()
    {
        // Arrange
        var student = CreateStudent();
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(3));
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal("not_started", dto.AttemptStatus);
        Assert.Equal(0, dto.TabSwitchCount);
        Assert.Equal(0, dto.CopyPasteAttemptCount);
    }

    [Fact]
    public async Task Handle_WhenStudentHasNoAttemptAndDueDatePassed_ReturnsMissed()
    {
        // Arrange
        var student = CreateStudent();
        var quiz = CreateQuiz(dueDate: DateTimeOffset.UtcNow.AddDays(-1));
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("missed", Assert.Single(result.Data!.Students).AttemptStatus);
    }

    [Fact]
    public async Task Handle_WhenStudentHasNoAttemptAndNoDueDateSet_ReturnsNotStarted()
    {
        // Arrange
        var student = CreateStudent();
        var quiz = CreateQuiz(dueDate: null);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("not_started", Assert.Single(result.Data!.Students).AttemptStatus);
    }

    #endregion

    #region Status derivation - with attempt

    [Fact]
    public async Task Handle_WhenAttemptInProgress_ReturnsInProgressStatusWithNoScore()
    {
        // Arrange
        var student = CreateStudent();
        var attempt = CreateAttempt(student.Id, QuizAttemptStatus.InProgress);
        var quiz = CreateQuiz(attempts: [attempt]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal("in_progress", dto.AttemptStatus);
        Assert.Null(dto.Score);
    }

    [Fact]
    public async Task Handle_WhenAttemptSubmitted_ReturnsSubmittedStatusAndCountsPendingWrittenAnswers()
    {
        // Arrange
        var student = CreateStudent();
        var attempt = CreateAttempt(student.Id, QuizAttemptStatus.Submitted, answers:
        [
            new AttemptAnswer { QuestionId = 1, Score = null },  // pending
            new AttemptAnswer { QuestionId = 2, Score = null },  // pending
            new AttemptAnswer { QuestionId = 3, Score = 5m }     // already graded (e.g. MCQ)
        ]);
        var quiz = CreateQuiz(attempts: [attempt]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal("submitted", dto.AttemptStatus);
        Assert.Equal(2, dto.PendingWrittenCount);
        Assert.Null(dto.Score); // no score shown until fully graded
    }

    [Fact]
    public async Task Handle_WhenAttemptGraded_ReturnsGradedStatusWithScoreAndNoPendingCount()
    {
        // Arrange
        var student = CreateStudent();
        var attempt = CreateAttempt(student.Id, QuizAttemptStatus.Graded, degree: 85m,
            submittedAt: DateTimeOffset.UtcNow.AddDays(-1));
        var quiz = CreateQuiz(totalDegree: 100m, attempts: [attempt]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal("graded", dto.AttemptStatus);
        Assert.Equal(85m, dto.Score);
        Assert.Equal(0, dto.PendingWrittenCount);
        Assert.Equal(attempt.SubmittedAt, dto.SubmittedAt);
    }

    [Fact]
    public async Task Handle_WhenAttemptExists_MapsSecurityEventCountsFromAttempt()
    {
        // Arrange
        var student = CreateStudent();
        var attempt = CreateAttempt(student.Id, QuizAttemptStatus.InProgress, tabSwitchCount: 3, copyPasteCount: 2);
        var quiz = CreateQuiz(attempts: [attempt]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal(3, dto.TabSwitchCount);
        Assert.Equal(2, dto.CopyPasteAttemptCount);
    }

    #endregion

    #region Mapping

    [Fact]
    public async Task Handle_MapsStudentNameByTrimmingFirstAndLastName()
    {
        // Arrange
        var student = CreateStudent(firstName: "Sara", lastName: "Ali");
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("Sara Ali", Assert.Single(result.Data!.Students).StudentName);
    }

    [Fact]
    public async Task Handle_MapsQuizLevelFieldsCorrectly()
    {
        // Arrange
        var quiz = CreateQuiz(id: 9, title: "Final Exam", totalDegree: 60m);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(CreateStudent());

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(9, result.Data!.QuizId);
        Assert.Equal("Final Exam", result.Data.Title);
        Assert.Equal(60m, result.Data.TotalDegree);
    }

    [Fact]
    public async Task Handle_EachStudentDtoGetsQuizTotalDegreeRegardlessOfAttemptStatus()
    {
        // Arrange
        var student = CreateStudent();
        var quiz = CreateQuiz(totalDegree: 75m);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(75m, Assert.Single(result.Data!.Students).TotalDegree);
    }

    #endregion

    #region TotalStudents vs TotalCount distinction

    [Fact]
    public async Task Handle_TotalStudentsReflectsAllEnrolledStudentsIgnoringFilters()
    {
        // Arrange
        var s1 = CreateStudent(firstName: "Sara");
        var s2 = CreateStudent(firstName: "Omar");
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupComprehensiveStudents(s1, s2);

        var query = ValidQuery with { Search = "Sara" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Data!.TotalStudents); // unfiltered total
        Assert.Equal(1, result.Data.TotalCount);      // filtered count
        Assert.Single(result.Data.Students);
    }

    #endregion

    #region Search filtering

    [Fact]
    public async Task Handle_WhenSearchProvided_FiltersByStudentNameCaseInsensitively()
    {
        // Arrange
        var sara = CreateStudent(firstName: "Sara", lastName: "Ali");
        var omar = CreateStudent(firstName: "Omar", lastName: "Khaled");
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupComprehensiveStudents(sara, omar);

        var query = ValidQuery with { Search = "sara" }; // lowercase, should still match

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal("Sara Ali", dto.StudentName);
    }

    [Fact]
    public async Task Handle_WhenSearchMatchesNothing_ReturnsEmptyStudentsList()
    {
        // Arrange
        var student = CreateStudent(firstName: "Sara");
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupComprehensiveStudents(student);

        var query = ValidQuery with { Search = "NoMatch" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Data!.Students);
        Assert.Equal(0, result.Data.TotalCount);
    }

    #endregion

    #region Status filtering

    [Fact]
    public async Task Handle_WhenStatusFilterProvided_ReturnsOnlyMatchingStudents()
    {
        // Arrange
        var notStarted = CreateStudent();
        var inProgressStudent = CreateStudent();
        var attempt = CreateAttempt(inProgressStudent.Id, QuizAttemptStatus.InProgress);

        var quiz = CreateQuiz(attempts: [attempt]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(notStarted, inProgressStudent);

        var query = ValidQuery with { Status = "in_progress" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Data!.Students);
        Assert.Equal("in_progress", dto.AttemptStatus);
    }

    #endregion

    #region Summary counts computed after filters

    [Fact]
    public async Task Handle_SummaryCountsAreComputedAfterFiltersAreApplied()
    {
        // Arrange
        var sara = CreateStudent(firstName: "Sara");
        var saraAttempt = CreateAttempt(sara.Id, QuizAttemptStatus.Graded, degree: 90m);

        var omar = CreateStudent(firstName: "Omar");
        var omarAttempt = CreateAttempt(omar.Id, QuizAttemptStatus.Submitted);

        var quiz = CreateQuiz(attempts: [saraAttempt, omarAttempt]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(sara, omar);

        var query = ValidQuery with { Search = "Sara" }; // narrows down to only Sara (graded)

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - counts reflect only the filtered set (Sara), not both students
        Assert.Equal(1, result.Data!.SubmittedCount); // graded counts as "submitted" too
        Assert.Equal(1, result.Data.GradedCount);
        Assert.Equal(0, result.Data.PendingGradingCount);
        Assert.Equal(2, result.Data.TotalStudents); // but total students remains unaffected
    }

    [Fact]
    public async Task Handle_SubmittedCountIncludesBothSubmittedAndGradedStatuses()
    {
        // Arrange
        var s1 = CreateStudent();
        var attempt1 = CreateAttempt(s1.Id, QuizAttemptStatus.Submitted);

        var s2 = CreateStudent();
        var attempt2 = CreateAttempt(s2.Id, QuizAttemptStatus.Graded, degree: 70m);

        var s3 = CreateStudent();
        var attempt3 = CreateAttempt(s3.Id, QuizAttemptStatus.InProgress);

        var quiz = CreateQuiz(attempts: [attempt1, attempt2, attempt3]);
        SetupQuiz(quiz);
        SetupComprehensiveStudents(s1, s2, s3);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Data!.SubmittedCount); // Submitted + Graded
        Assert.Equal(1, result.Data.PendingGradingCount); // only Submitted
        Assert.Equal(1, result.Data.GradedCount);
    }

    #endregion

    #region Pagination

    [Fact]
    public async Task Handle_PaginatesFilteredStudentsCorrectly()
    {
        // Arrange
        var students = Enumerable.Range(1, 25).Select(_ => CreateStudent()).ToArray();
        var quiz = CreateQuiz();
        SetupQuiz(quiz);
        SetupComprehensiveStudents(students);

        var query = ValidQuery with { Page = 2, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.Data!.Students.Count);
        Assert.Equal(25, result.Data.TotalCount);
        Assert.Equal(2, result.Data.Page);
        Assert.Equal(10, result.Data.PageSize);
    }

    [Fact]
    public async Task Handle_WhenPageSizeExceedsMax_ClampsToOneHundred()
    {
        // Arrange
        SetupQuiz(CreateQuiz());
        SetupComprehensiveStudents(CreateStudent());
        var query = ValidQuery with { PageSize = 500 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(100, result.Data!.PageSize);
    }

    [Fact]
    public async Task Handle_WhenPageIsZeroOrNegative_ClampsToPageOne()
    {
        // Arrange
        SetupQuiz(CreateQuiz());
        SetupComprehensiveStudents(CreateStudent());
        var query = ValidQuery with { Page = -3 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Data!.Page);
    }

    #endregion
}
