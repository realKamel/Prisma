using System.Security.Claims;
using Ardalis.Specification;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.Assistants.Queries;

public class GetAssistantDashboardQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _userManager;
    private readonly GetAssistantDashboardQueryHandler _handler;

    private readonly IRepository<Enrollment, int> _enrollmentRepo;
    private readonly IRepository<Lesson, int> _lessonRepo;
    private readonly IRepository<QuizAttempt, int> _quizAttemptRepo;
    private readonly IRepository<AssignmentSubmission, int> _submissionRepo;
    private readonly IRepository<AuditLog, int> _auditRepo;

    private static readonly Guid TestUserId = Guid.NewGuid();
    private const string TestEmail = "assistant@example.com";

    public GetAssistantDashboardQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUserService>();

        _userManager = Substitute.For<IIdentityService>();

        _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
        _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
        _quizAttemptRepo = Substitute.For<IRepository<QuizAttempt, int>>();
        _submissionRepo = Substitute.For<IRepository<AssignmentSubmission, int>>();
        _auditRepo = Substitute.For<IRepository<AuditLog, int>>();

        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_quizAttemptRepo);
        _unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>().Returns(_submissionRepo);
        _unitOfWork.GetOrCreateRepository<AuditLog, int>().Returns(_auditRepo);

        _currentUser.UserId.Returns(TestUserId);
        _currentUser.Email.Returns(TestEmail);

        _handler = new GetAssistantDashboardQueryHandler(_unitOfWork, _currentUser, _userManager);

        // Baseline "nothing happened" defaults — individual tests override what they care about.
        _enrollmentRepo.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _lessonRepo.CountAsync(Arg.Any<ISpecification<Lesson>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _quizAttemptRepo.CountAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _quizAttemptRepo.ListAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(new List<QuizAttempt>());
        _submissionRepo.CountAsync(Arg.Any<ISpecification<AssignmentSubmission>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _auditRepo.ListAsync(Arg.Any<ISpecification<AuditLog>>(), Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog>());

        _userManager.FindByIdAsync(TestUserId).Returns((User)null!);
        _userManager.GetClaimsAsync(Arg.Any<User>()).Returns(new List<Claim>());
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsFullyPopulatedDashboard()
    {
        // Arrange
        var assistant = new User
        {
            Id = TestUserId,
            FirstName = "Sara",
            LastName = "Ahmed",
            UserName = TestEmail,
            Email = TestEmail
        };

        _enrollmentRepo.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(50, 40); // activeNow=50, activeLastWeek=40 -> delta +10

        _quizAttemptRepo.CountAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(20);

        var quiz = new Quiz { TotalDegree = 100 };
        var gradedThisWeek = new List<QuizAttempt>
        {
            new() { Degree = 80, Quiz = quiz }, // pass
            new() { Degree = 30, Quiz = quiz }, // fail
        };
        var gradedLastWeek = new List<QuizAttempt>
        {
            new() { Degree = 20, Quiz = quiz }, // fail
            new() { Degree = 10, Quiz = quiz }, // fail
        };

        _quizAttemptRepo.ListAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(gradedThisWeek, gradedLastWeek);

        _submissionRepo.CountAsync(Arg.Any<ISpecification<AssignmentSubmission>>(), Arg.Any<CancellationToken>())
            .Returns(7);

        _lessonRepo.CountAsync(Arg.Any<ISpecification<Lesson>>(), Arg.Any<CancellationToken>())
            .Returns(15, 3); // totalLessons=15, newLessonsThisWeek=3

        var logCreatedAt = DateTimeOffset.UtcNow;
        var logs = new List<AuditLog>
        {
            new() { Id = 1, Action = "Update", TableName = "Lessons", CreatedAt = logCreatedAt },
        };
        _auditRepo.ListAsync(Arg.Any<ISpecification<AuditLog>>(), Arg.Any<CancellationToken>())
            .Returns(logs);

        _userManager.FindByIdAsync(TestUserId).Returns(assistant);
        _userManager.GetClaimsAsync(assistant).Returns(new List<Claim>
        {
            new(AppClaims.PermissionsClaim, AppClaims.Policies.CanManageEnrollments),
            new(AppClaims.PermissionsClaim, AppClaims.Policies.CanViewReports),
        });

        // Act
        var result = await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        result.Value.Teacher.Name.Should().Be("Sara Ahmed");
        result.Value.Teacher.SupervisorName.Should().Be("أ. أحمد مصطفى");

        result.Value.Kpis.Should().HaveCount(4);

        var studentsKpi = result.Value.Kpis.Single(k => k.Id == "students");
        studentsKpi.Value.Should().Be(50);
        studentsKpi.Delta.Should().Be(10);
        studentsKpi.Trend.Should().Be("up");
        studentsKpi.Variant.Should().Be("purple");

        var quizzesKpi = result.Value.Kpis.Single(k => k.Id == "quizzes");
        quizzesKpi.Value.Should().Be(20);
        // pass rate this week = 1/2 = 0.5, last week = 0/2 = 0 -> delta 0.5
        quizzesKpi.Delta.Should().BeApproximately(0.5, 0.0001);
        quizzesKpi.Trend.Should().Be("up");

        var assignmentsKpi = result.Value.Kpis.Single(k => k.Id == "assignments");
        assignmentsKpi.Value.Should().Be(7);
        assignmentsKpi.Delta.Should().Be(0);
        assignmentsKpi.Trend.Should().Be("down");

        var lessonsKpi = result.Value.Kpis.Single(k => k.Id == "lessons");
        lessonsKpi.Value.Should().Be(15);
        lessonsKpi.Delta.Should().Be(3);
        lessonsKpi.Trend.Should().Be("up");

        result.Value.Activities.Should().HaveCount(1);
        result.Value.Activities[0].Id.Should().Be(1);
        result.Value.Activities[0].Action.Should().Be("Update");
        result.Value.Activities[0].TableName.Should().Be("Lessons");
        result.Value.Activities[0].CreatedAt.Should().Be(logCreatedAt);

        result.Value.Permissions.Should().HaveCount(4);
        result.Value.Permissions.Single(p => p.Id == "students").Status.Should().Be("on");
        result.Value.Permissions.Single(p => p.Id == "reports").Status.Should().Be("on");
        result.Value.Permissions.Single(p => p.Id == "content").Status.Should().Be("off");
        result.Value.Permissions.Single(p => p.Id == "grading").Status.Should().Be("off");
    }

    [Fact]
    public async Task Handle_WhenAssistantNotFound_ReturnsEmptyTeacherNameAndAllPermissionsOff()
    {
        // Arrange
        _userManager.FindByIdAsync(TestUserId).Returns((User)null!);

        // Act
        var result = await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Teacher.Name.Should().BeEmpty();
        result.Value.Teacher.SupervisorName.Should().Be("أ. أحمد مصطفى");
        result.Value.Permissions.Should().OnlyContain(p => p.Status == "off");

        await _userManager.DidNotReceive().GetClaimsAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WhenActiveStudentsDecreased_SetsStudentsTrendToDown()
    {
        // Arrange
        _enrollmentRepo.CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(30, 45); // activeNow=30, activeLastWeek=45 -> delta -15

        // Act
        var result = await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);

        // Assert
        var studentsKpi = result.Value.Kpis.Single(k => k.Id == "students");
        studentsKpi.Delta.Should().Be(-15);
        studentsKpi.Trend.Should().Be("down");
    }

    [Fact]
    public async Task Handle_WhenNoGradedQuizAttempts_ComputesZeroPassRateDeltaWithoutDivideByZero()
    {
        // Arrange
        _quizAttemptRepo.ListAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(new List<QuizAttempt>(), new List<QuizAttempt>());

        // Act
        var act = async () => await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);

        // Assert
        var result = await act.Should().NotThrowAsync();
        var quizzesKpi = result.Subject.Value.Kpis.Single(k => k.Id == "quizzes");
        quizzesKpi.Delta.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNoNewLessonsThisWeek_SetsLessonsTrendToDown()
    {
        // Arrange
        _lessonRepo.CountAsync(Arg.Any<ISpecification<Lesson>>(), Arg.Any<CancellationToken>())
            .Returns(10, 0);

        // Act
        var result = await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);

        // Assert
        var lessonsKpi = result.Value.Kpis.Single(k => k.Id == "lessons");
        lessonsKpi.Value.Should().Be(10);
        lessonsKpi.Delta.Should().Be(0);
        lessonsKpi.Trend.Should().Be("down");
    }

    [Fact]
    public async Task Handle_WhenAuditLogCreatedAtIsNull_FallsBackToUtcNow()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new() { Id = 5, Action = "Delete", TableName = "Enrollments", CreatedAt = null },
        };
        _auditRepo.ListAsync(Arg.Any<ISpecification<AuditLog>>(), Arg.Any<CancellationToken>())
            .Returns(logs);

        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);
        var after = DateTimeOffset.UtcNow;

        // Assert
        var activity = result.Value.Activities.Single();
        activity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}