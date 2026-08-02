using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Assistants.Queries.GetAssistantDetailedLogs;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AuditLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Assistants.Queries;

public class GetAssistantDetailedLogsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IRepository<AuditLog, int> _auditLogRepo = Substitute.For<IRepository<AuditLog, int>>();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<IRepository<Student, Guid>>();
    private readonly IRepository<Enrollment, int> _enrollmentRepo = Substitute.For<IRepository<Enrollment, int>>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly GetAssistantDetailedLogsQueryHandler _sut;

    public GetAssistantDetailedLogsQueryHandlerTests()
    {
        // ربط الـ Repositories بالـ UnitOfWork
        _unitOfWork.GetOrCreateRepository<AuditLog, int>().Returns(_auditLogRepo);
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _unitOfWork.GetOrCreateRepository<Enrollment, int>().Returns(_enrollmentRepo);
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);

        _sut = new GetAssistantDetailedLogsQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetAssistantDetailedLogsQuery(Take: 10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WithValidEnrollmentLog_MapsDetailsAndComputesMetaCorrectly()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.CreateVersion7());
        _currentUserService.Email.Returns("assistant@prisma.com");

        var query = new GetAssistantDetailedLogsQuery(Take: 10);
        var today = DateTimeOffset.UtcNow;

        var fakeLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                Action = "GrantLessonAccess",
                TableName = "Enrollment",
                EntityId = "100",
                CreatedAt = today
            }
        };

        var fakeStudent = new Student
        {
            FirstName = "محمد",
            LastName = "علي",
            UserName = "mohamed_ali",
            AcademicYear = new() { Title = "الصف الثالث الإعدادي" }
        };

        var fakeEnrollment = new Enrollment
        {
            Id = 100,
            Student = fakeStudent,
            Lesson = new() { Title = "مراجعة الجبر الجملية" },
            ExpiresAt = today.AddDays(5)
        };

        _auditLogRepo.ListAsync(Arg.Any<RecentAssistantLogsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        _enrollmentRepo.FirstOrDefaultAsync(Arg.Any<EnrollmentWithStudentAndLessonSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeEnrollment);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Logs.Should().HaveCount(1);

        var logItem = result.Value.Logs.First();
        logItem.Type.Should().Be("grant");
        logItem.Student.Should().Be("محمد علي");
        logItem.Grade.Should().Be("الصف الثالث الإعدادي");
        logItem.Detail.Should().Be("مراجعة الجبر الجملية");
        logItem.Sub.Should().Be("صلاحية 4 أيام");
        logItem.Date.Should().Be("اليوم");

        // التأكد من الـ Meta الإحصائية
        result.Value.Meta.TotalThisMonth.Should().Be(1);
        result.Value.Meta.Granted.Should().Be(1);
        result.Value.Meta.Revoked.Should().Be(0);
        result.Value.Meta.SuccessRate.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithStudentLog_MapsStudentProfileViewCorrectly()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.CreateVersion7());
        _currentUserService.Email.Returns("assistant@prisma.com");

        var query = new GetAssistantDetailedLogsQuery(Take: 5);
        var studentGuid = Guid.CreateVersion7();

        var fakeLogs = new List<AuditLog>
        {
            new()
            {
                Id = 2,
                Action = "ViewProfile",
                TableName = "Student",
                EntityId = studentGuid.ToString(),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) // أمس
            }
        };

        var fakeStudent = new Student
        {
            FirstName = "سارة",
            LastName = "أحمد",
            AcademicYear = new() { Title = "الصف الأول الثانوي" }
        };

        _auditLogRepo.ListAsync(Arg.Any<RecentAssistantLogsSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        _studentRepo.FirstOrDefaultAsync(Arg.Any<StudentWithAcademicYearSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeStudent);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var logItem = result.Value.Logs.First();
        logItem.Type.Should().Be("view");
        logItem.Student.Should().Be("سارة أحمد");
        logItem.Detail.Should().Be("ملف الطالب");
        logItem.Sub.Should().Be("ViewProfile");
        logItem.Grade.Should().Be("الصف الأول الثانوي");
        logItem.Date.Should().Be("أمس");
    }

    [Fact]
    public async Task Handle_WhenNoLogsExist_ReturnsEmptyLogsAndDefaultMeta()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.CreateVersion7());
        _currentUserService.Email.Returns("assistant@prisma.com");

        _auditLogRepo.ListAsync(Arg.Any<RecentAssistantLogsSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog>());

        var query = new GetAssistantDetailedLogsQuery(Take: 10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Logs.Should().BeEmpty();
        result.Value.Meta.TotalThisMonth.Should().Be(0);
        result.Value.Meta.SuccessRate.Should().Be(100); // طبقاً لكود الـ Handler: لو الكاونت 0 بيرجع 100
    }
}