using System.Security.Claims;
using Ardalis.Specification;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Assistants.Queries.GetAssistantDashboard;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using Prisma.Domain.Specifications.QuizAttemptSpecs;

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
    private readonly IRepository<AuditLog, long> _auditRepo;

    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();
    private const string TestEmail = "assistant@example.com";

    private readonly Assistant _assistant;
    private readonly Prisma.Domain.Entities.UserAggregate.Teacher _teacher;

    public GetAssistantDashboardQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUserService>();

        _userManager = Substitute.For<IIdentityService>();

        _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
        _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
        _quizAttemptRepo = Substitute.For<IRepository<QuizAttempt, int>>();
        _submissionRepo = Substitute.For<IRepository<AssignmentSubmission, int>>();
        _auditRepo = Substitute.For<IRepository<AuditLog, long>>();

        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<QuizAttempt, int>().Returns(_quizAttemptRepo);
        _unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>().Returns(_submissionRepo);
        _unitOfWork.GetOrCreateRepository<AuditLog, long>().Returns(_auditRepo);

        _currentUser.UserId.Returns(TestUserId);
        _currentUser.Email.Returns(TestEmail);

        _handler = new GetAssistantDashboardQueryHandler(_unitOfWork, _currentUser, _userManager);

        _assistant = new Assistant
        {
            Id = TestUserId,
            FirstName = "Sara",
            SecondName = "Ahmed",
            UserName = TestEmail,
            Email = TestEmail,
            TeacherId = TeacherId,
        };

        _teacher = new Prisma.Domain.Entities.UserAggregate.Teacher
        {
            Id = TeacherId,
            FirstName = "أحمد",
            SecondName = "مصطفى",
        };

        _userManager.FindByIdAsync(TestUserId).Returns(_assistant);
        _userManager.FindByIdAsync(TeacherId).Returns(_teacher);
        _userManager.GetClaimsAsync(Arg.Any<User>()).Returns(new List<Claim>());

        // Baseline "nothing happened" defaults — individual tests override what they care about.
        _enrollmentRepo
            .CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _lessonRepo
            .CountAsync(Arg.Any<ISpecification<Lesson>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _quizAttemptRepo
            .CountAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _quizAttemptRepo
            .ListAsync(
                Arg.Any<
                    QuizAttemptWithProjectionSpec<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new List<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>(),
                new List<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>()
            );
        _submissionRepo
            .CountAsync(
                Arg.Any<ISpecification<AssignmentSubmission>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(0);
        _auditRepo
            .ListAsync(
                Arg.Any<AuditLogWithProjectionSpec<ActivityItem>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<ActivityItem>());
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _currentUser.UserId.Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(
            new GetAssistantDashboardQuery(),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAssistant_ReturnsUnauthorized()
    {
        // Arrange
        _userManager.FindByIdAsync(TestUserId).Returns(new User { Id = TestUserId });

        // Act
        var result = await _handler.Handle(
            new GetAssistantDashboardQuery(),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsFullyPopulatedDashboard()
    {
        // Arrange
        _enrollmentRepo
            .CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(50, 40); // activeNow=50, activeLastWeek=40 -> delta +10

        _quizAttemptRepo
            .CountAsync(Arg.Any<ISpecification<QuizAttempt>>(), Arg.Any<CancellationToken>())
            .Returns(20);

        var gradedThisWeek = new List<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>
        {
            new(Degree: 80, TotalDegree: 100), // pass
            new(Degree: 30, TotalDegree: 100), // fail
        };
        var gradedLastWeek = new List<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>
        {
            new(Degree: 20, TotalDegree: 100), // fail
            new(Degree: 10, TotalDegree: 100), // fail
        };

        _quizAttemptRepo
            .ListAsync(
                Arg.Any<
                    QuizAttemptWithProjectionSpec<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(gradedThisWeek, gradedLastWeek);

        _submissionRepo
            .CountAsync(
                Arg.Any<ISpecification<AssignmentSubmission>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(7);

        _lessonRepo
            .CountAsync(Arg.Any<ISpecification<Lesson>>(), Arg.Any<CancellationToken>())
            .Returns(15, 3); // totalLessons=15, newLessonsThisWeek=3

        var logCreatedAt = DateTimeOffset.UtcNow;
        var activities = new List<ActivityItem>
        {
            new(Id: 1, Action: "Update", TableName: "Lessons", CreatedAt: logCreatedAt),
        };
        _auditRepo
            .ListAsync(
                Arg.Any<AuditLogWithProjectionSpec<ActivityItem>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(activities);

        _userManager
            .GetClaimsAsync(_assistant)
            .Returns(
                new List<Claim>
                {
                    new(AppClaims.PermissionsClaim, AppClaims.Policies.CanManageEnrollments),
                    new(AppClaims.PermissionsClaim, AppClaims.Policies.CanViewReports),
                }
            );

        // Act
        var result = await _handler.Handle(
            new GetAssistantDashboardQuery(),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Teacher.Name.Should().Be("Sara Ahmed");
        result.Value.Teacher.SupervisorName.Should().Be("أحمد مصطفى");

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
    public async Task Handle_WhenActiveStudentsDecreased_SetsStudentsTrendToDown()
    {
        // Arrange
        _enrollmentRepo
            .CountAsync(Arg.Any<ISpecification<Enrollment>>(), Arg.Any<CancellationToken>())
            .Returns(30, 45); // activeNow=30, activeLastWeek=45 -> delta -15

        // Act
        var result = await _handler.Handle(
            new GetAssistantDashboardQuery(),
            CancellationToken.None
        );

        // Assert
        var studentsKpi = result.Value.Kpis.Single(k => k.Id == "students");
        studentsKpi.Delta.Should().Be(-15);
        studentsKpi.Trend.Should().Be("down");
    }

    [Fact]
    public async Task Handle_WhenNoGradedQuizAttempts_ComputesZeroPassRateDeltaWithoutDivideByZero()
    {
        // Arrange
        _quizAttemptRepo
            .ListAsync(
                Arg.Any<
                    QuizAttemptWithProjectionSpec<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>
                >(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new List<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>(),
                new List<GetAssistantDashboardQueryHandler.QuizAttemptScoreInfo>()
            );

        // Act
        var act = async () =>
            await _handler.Handle(new GetAssistantDashboardQuery(), CancellationToken.None);

        // Assert
        var result = await act.Should().NotThrowAsync();
        var quizzesKpi = result.Subject.Value.Kpis.Single(k => k.Id == "quizzes");
        quizzesKpi.Delta.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNoNewLessonsThisWeek_SetsLessonsTrendToDown()
    {
        // Arrange
        _lessonRepo
            .CountAsync(Arg.Any<ISpecification<Lesson>>(), Arg.Any<CancellationToken>())
            .Returns(10, 0);

        // Act
        var result = await _handler.Handle(
            new GetAssistantDashboardQuery(),
            CancellationToken.None
        );

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
        _auditRepo
            .ListAsync(
                Arg.Any<AuditLogWithProjectionSpec<ActivityItem>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(callInfo =>
            {
                var spec = callInfo.ArgAt<AuditLogWithProjectionSpec<ActivityItem>>(0);
                // The projection already applied the UtcNow fallback when projecting.
                return new List<ActivityItem>
                {
                    new(5, "Delete", "Enrollments", DateTimeOffset.UtcNow),
                };
            });

        var before = DateTimeOffset.UtcNow;

        // Act
        var result = await _handler.Handle(
            new GetAssistantDashboardQuery(),
            CancellationToken.None
        );
        var after = DateTimeOffset.UtcNow;

        // Assert
        var activity = result.Value.Activities.Single();
        activity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
