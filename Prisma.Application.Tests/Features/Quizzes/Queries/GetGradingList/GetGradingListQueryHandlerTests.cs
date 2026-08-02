using System;
using System.Collections.Generic;
using System.Text;
using NSubstitute;
using Prisma.Application.Features.Quizzes.Queries.GetGradingList;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Tests.Features.Quizzes.Queries.GetGradingList;

public class GetGradingListQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<QuizAttempt, int> _attemptRepository = Substitute.For<IRepository<QuizAttempt, int>>();
    private readonly GetGradingListQueryHandler _handler;

    private static readonly GetGradingListQuery ValidQuery =
        new(QuizScope.ComprehensiveExam, Search: null, Status: null, QuizId: null);

    public GetGradingListQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_attemptRepository);
        _handler = new GetGradingListQueryHandler(_unitOfWork);
    }

    #region Helpers

    private static Student CreateStudent(
        string firstName = "Sara", string? secondName = null, string? thirdName = null, string lastName = "Ali") =>
        new() { Id = Guid.NewGuid(), FirstName = firstName, SecondName = secondName, ThirdName = thirdName, LastName = lastName };

    private static Quiz CreateQuiz(int id = 1, string title = "Quiz", decimal totalDegree = 100m) =>
        new() { Id = id, Title = title, TotalDegree = totalDegree };

    private static QuizAttempt CreateAttempt(
        int id = 1,
        Student? student = null,
        Quiz? quiz = null,
        QuizAttemptStatus status = QuizAttemptStatus.Submitted,
        decimal degree = 0m,
        decimal penaltyScore = 0m,
        int tabSwitchCount = 0,
        int copyPasteCount = 0,
        List<AttemptAnswer>? answers = null)
    {
        var s = student ?? CreateStudent();
        var q = quiz ?? CreateQuiz();
        return new QuizAttempt
        {
            Id = id,
            Student = s,
            StudentId = s.Id,
            Quiz = q,
            QuizId = q.Id,
            Status = status,
            Degree = degree,
            PenaltyScore = penaltyScore,
            TabSwitchCount = tabSwitchCount,
            CopyPasteAttemptCount = copyPasteCount,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-1),
            Answers = answers ?? new List<AttemptAnswer>()
        };
    }

    private void SetupAttempts(params QuizAttempt[] attempts) =>
        _attemptRepository
            .ListAsync(Arg.Any<GradingAttemptsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(attempts.ToList());

    #endregion

    #region Search filtering

    [Fact]
    public async Task Handle_WhenSearchMatchesStudentFirstName_ReturnsMatchingAttempt()
    {
        // Arrange
        var attempt = CreateAttempt(student: CreateStudent(firstName: "Mona"));
        SetupAttempts(attempt, CreateAttempt(id: 2, student: CreateStudent(firstName: "Omar")));

        var query = ValidQuery with { Search = "Mona" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task Handle_WhenSearchMatchesStudentSecondName_ReturnsMatchingAttempt()
    {
        // Arrange - confirms the search now covers SecondName/ThirdName, matching the display name
        var attempt = CreateAttempt(student: CreateStudent(firstName: "Sara", secondName: "Mohamed"));
        SetupAttempts(attempt);

        var query = ValidQuery with { Search = "Mohamed" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task Handle_WhenSearchMatchesQuizTitle_ReturnsMatchingAttempt()
    {
        // Arrange
        var attempt = CreateAttempt(quiz: CreateQuiz(title: "Algebra Midterm"));
        SetupAttempts(attempt, CreateAttempt(id: 2, quiz: CreateQuiz(title: "Geometry Final")));

        var query = ValidQuery with { Search = "algebra" }; // case-insensitive

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task Handle_WhenSearchMatchesNothing_ReturnsEmptyItems()
    {
        // Arrange
        SetupAttempts(CreateAttempt(student: CreateStudent(firstName: "Sara")));

        var query = ValidQuery with { Search = "NoMatch" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    #endregion

    #region Status filtering

    [Fact]
    public async Task Handle_WhenStatusIsSubmitted_ReturnsOnlySubmittedAttempts()
    {
        // Arrange
        SetupAttempts(
            CreateAttempt(id: 1, status: QuizAttemptStatus.Submitted),
            CreateAttempt(id: 2, status: QuizAttemptStatus.Graded, degree: 80m));

        var query = ValidQuery with { Status = "submitted" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("submitted", item.Status);
    }

    [Fact]
    public async Task Handle_WhenStatusIsGraded_ReturnsOnlyGradedAttempts()
    {
        // Arrange
        SetupAttempts(
            CreateAttempt(id: 1, status: QuizAttemptStatus.Submitted),
            CreateAttempt(id: 2, status: QuizAttemptStatus.Graded, degree: 80m));

        var query = ValidQuery with { Status = "graded" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("graded", item.Status);
    }

    [Fact]
    public async Task Handle_WhenStatusIsAll_ReturnsAllAttempts()
    {
        // Arrange
        SetupAttempts(
            CreateAttempt(id: 1, status: QuizAttemptStatus.Submitted),
            CreateAttempt(id: 2, status: QuizAttemptStatus.Graded, degree: 80m));

        var query = ValidQuery with { Status = "all" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenStatusIsNull_ReturnsAllAttempts()
    {
        // Arrange
        SetupAttempts(
            CreateAttempt(id: 1, status: QuizAttemptStatus.Submitted),
            CreateAttempt(id: 2, status: QuizAttemptStatus.Graded, degree: 80m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenStatusIsUnrecognizedValue_ReturnsAllAttemptsUnfiltered()
    {
        // Arrange - the switch's `_ => filtered` branch means an unknown status is a silent no-op
        SetupAttempts(
            CreateAttempt(id: 1, status: QuizAttemptStatus.Submitted),
            CreateAttempt(id: 2, status: QuizAttemptStatus.Graded, degree: 80m));

        var query = ValidQuery with { Status = "some_typo" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value!.Items.Count);
    }

    #endregion

    #region Mapping

    [Fact]
    public async Task Handle_MapsStudentNameFromAllFourNameParts()
    {
        // Arrange
        var student = CreateStudent(firstName: "Sara", secondName: "Mohamed", thirdName: "Ahmed", lastName: "Ali");
        SetupAttempts(CreateAttempt(student: student));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("Sara Mohamed Ahmed Ali", Assert.Single(result.Value!.Items).StudentName);
    }

    [Fact]
    public async Task Handle_WhenMiddleNamesAreNull_JoinsOnlyNonEmptyNameParts()
    {
        // Arrange
        var student = CreateStudent(firstName: "Sara", secondName: null, thirdName: null, lastName: "Ali");
        SetupAttempts(CreateAttempt(student: student));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert - string.Join with Where(NotNullOrWhiteSpace) skips null parts entirely, no double spaces
        Assert.Equal("Sara Ali", Assert.Single(result.Value!.Items).StudentName);
    }


    [Fact]
    public async Task Handle_WhenAttemptIsSubmitted_ScoreIsNull()
    {
        // Arrange
        SetupAttempts(CreateAttempt(status: QuizAttemptStatus.Submitted, degree: 50m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert - Degree may be internally computed but shouldn't surface until Graded
        Assert.Null(Assert.Single(result.Value!.Items).Score);
    }

    [Fact]
    public async Task Handle_WhenOnlySecondNameIsNull_SkipsItWithoutLeavingGap()
    {
        // Arrange
        var student = CreateStudent(firstName: "Sara", secondName: null, thirdName: "Ahmed", lastName: "Ali");
        SetupAttempts(CreateAttempt(student: student));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal("Sara Ahmed Ali", Assert.Single(result.Value!.Items).StudentName);
    }

    [Fact]
    public async Task Handle_WhenAttemptIsGraded_ScoreReflectsDegree()
    {
        // Arrange
        SetupAttempts(CreateAttempt(status: QuizAttemptStatus.Graded, degree: 75m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(75m, Assert.Single(result.Value!.Items).Score);
    }

    [Fact]
    public async Task Handle_MapsPenaltyScoreAndTotalDegreeFromQuiz()
    {
        // Arrange
        SetupAttempts(CreateAttempt(quiz: CreateQuiz(totalDegree: 60m), penaltyScore: 5m));

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal(5m, item.PenaltyScore);
        Assert.Equal(60m, item.TotalDegree);
    }

    [Fact]
    public async Task Handle_ComputesPendingWrittenCountFromUngradedAnswers()
    {
        // Arrange
        var attempt = CreateAttempt(answers:
        [
            new AttemptAnswer { QuestionId = 1, Score = 5m },
            new AttemptAnswer { QuestionId = 2, Score = null },
            new AttemptAnswer { QuestionId = 3, Score = null }
        ]);
        SetupAttempts(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.Equal(2, Assert.Single(result.Value!.Items).PendingWrittenCount);
    }

    #endregion

    #region HeldForSecurityReview

    [Fact]
    public async Task Handle_WhenSubmittedFullyGradedAndHasSecurityEvents_HeldForSecurityReviewIsTrue()
    {
        // Arrange
        var attempt = CreateAttempt(
            status: QuizAttemptStatus.Submitted,
            tabSwitchCount: 2,
            answers: [new AttemptAnswer { QuestionId = 1, Score = 5m }]); // fully graded
        SetupAttempts(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.True(Assert.Single(result.Value!.Items).HeldForSecurityReview);
    }

    [Fact]
    public async Task Handle_WhenSubmittedButHasPendingWrittenAnswer_HeldForSecurityReviewIsFalseEvenWithSecurityEvents()
    {
        // Arrange - not all answers graded yet, so this is a normal pending-grading case, not a security hold
        var attempt = CreateAttempt(
            status: QuizAttemptStatus.Submitted,
            copyPasteCount: 1,
            answers: [new AttemptAnswer { QuestionId = 1, Score = null }]);
        SetupAttempts(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(Assert.Single(result.Value!.Items).HeldForSecurityReview);
    }

    [Fact]
    public async Task Handle_WhenSubmittedFullyGradedButNoSecurityEvents_HeldForSecurityReviewIsFalse()
    {
        // Arrange - fully graded and Submitted with zero security events shouldn't normally happen
        // (QuizFinalizer would have marked it Graded), but the flag logic should still hold up defensively
        var attempt = CreateAttempt(
            status: QuizAttemptStatus.Submitted,
            tabSwitchCount: 0,
            copyPasteCount: 0,
            answers: [new AttemptAnswer { QuestionId = 1, Score = 5m }]);
        SetupAttempts(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(Assert.Single(result.Value!.Items).HeldForSecurityReview);
    }

    [Fact]
    public async Task Handle_WhenAttemptIsGraded_HeldForSecurityReviewIsFalseRegardlessOfSecurityEvents()
    {
        // Arrange - HeldForSecurityReview only applies to Submitted attempts, not Graded ones
        var attempt = CreateAttempt(status: QuizAttemptStatus.Graded, degree: 90m, tabSwitchCount: 5);
        SetupAttempts(attempt);

        // Act
        var result = await _handler.Handle(ValidQuery, CancellationToken.None);

        // Assert
        Assert.False(Assert.Single(result.Value!.Items).HeldForSecurityReview);
    }

    #endregion

    #region Pagination

    [Fact]
    public async Task Handle_PaginatesResultsCorrectly()
    {
        // Arrange
        var attempts = Enumerable.Range(1, 25).Select(i => CreateAttempt(id: i)).ToArray();
        SetupAttempts(attempts);

        var query = ValidQuery with { Page = 2, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.Value!.Items.Count);
        Assert.Equal(11, result.Value.Items.First().AttemptId);
        Assert.Equal(25, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_TotalCountReflectsFilteredResultsNotJustPagedItems()
    {
        // Arrange
        SetupAttempts(
            CreateAttempt(id: 1, status: QuizAttemptStatus.Submitted),
            CreateAttempt(id: 2, status: QuizAttemptStatus.Graded, degree: 80m),
            CreateAttempt(id: 3, status: QuizAttemptStatus.Submitted));

        var query = ValidQuery with { Status = "submitted", PageSize = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Value!.Items); // paged down to 1
        Assert.Equal(2, result.Value.TotalCount); // but total reflects both submitted attempts
    }

    [Fact]
    public async Task Handle_WhenPageSizeExceedsMax_ClampsToOneHundred()
    {
        // Arrange
        SetupAttempts(CreateAttempt());
        var query = ValidQuery with { PageSize = 500 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(100, result.Value!.PageSize);
    }

    [Fact]
    public async Task Handle_WhenPageIsZeroOrNegative_ClampsToPageOne()
    {
        // Arrange
        SetupAttempts(CreateAttempt());
        var query = ValidQuery with { Page = -1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Value!.Page);
    }

    #endregion
}
