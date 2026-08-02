using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Features.Admin.Queries.GetActivityLogsQuery;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Tests.Features.Admin.Queries;

public class GetActivityLogsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UserManager<User> _userManager;
    private readonly IRepository<AuditLog, int> _auditLogRepository = Substitute.For<IRepository<AuditLog, int>>();
    private readonly GetActivityLogsQueryHandler _sut;

    public GetActivityLogsQueryHandlerTests()
    {
        // ربط الـ Repository بالـ Unit of Work
        _unitOfWork.GetOrCreateRepository<AuditLog, int>().Returns(_auditLogRepository);

        // عمل Mock للـ UserManager لأننا بنحتاجه كـ Dependency في الـ Handler
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new GetActivityLogsQueryHandler(_unitOfWork, _userManager);
    }

    [Fact]
    public async Task Handle_WhenLogsHaveSystemUser_ReturnsSystemDisplayWithoutCallingUserManager()
    {
        // Arrange
        var query = new GetActivityLogsQuery(Skip: 0, Take: 2);
        var fakeLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                UserEmail = "system",
                Action = "UPDATE",
                TableName = "Lesson",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _auditLogRepository.ListAsync(Arg.Any<ActivityLogsFilterSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Events.Should().HaveCount(1);
        result.Value.Events[0].User.Should().Be("النظام");
        result.Value.Events[0].Role.Should().Be("system");

        // التأكد أن الـ UserManager لم يتم استدعاؤه لأن المستخدم هو النظام
        await _userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WhenLogHasNormalUser_FetchesUserAndRolesCorrectly()
    {
        // Arrange
        var query = new GetActivityLogsQuery(Skip: 0, Take: 2);
        var userEmail = "teacher@prisma.com";
        var fakeUser = new User { FirstName = "احمد", LastName = "علي", Email = userEmail };

        var fakeLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                UserEmail = userEmail,
                Action = "INSERT",
                TableName = "Lesson",
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _auditLogRepository.ListAsync(Arg.Any<ActivityLogsFilterSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        _userManager.FindByEmailAsync(userEmail).Returns(fakeUser);
        _userManager.GetRolesAsync(fakeUser).Returns(new List<string> { "Teacher" });

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var @event = result.Value.Events.First();
        @event.User.Should().Be("احمد علي");
        @event.Role.Should().Be("teacher"); // الـ Handler بيعمل ToLower() للـ Role
    }

    [Fact]
    public async Task Handle_WhenMoreLogsExistThanTake_SetsHasMoreToTrue()
    {
        // Arrange
        // الـ Handler بيطلب Take + 1 عشان يعرف فيه صفحات تانية ولا لأ
        var query = new GetActivityLogsQuery(Skip: 0, Take: 1);
        var fakeLogs = new List<AuditLog>
        {
            new() { Id = 1, UserEmail = "system" }, new() { Id = 2, UserEmail = "system" } // سجل إضافي
        };

        _auditLogRepository.ListAsync(Arg.Any<ActivityLogsFilterSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.HasMore.Should().BeTrue();
        result.Value.Events.Should().HaveCount(1); // يجب أن يعيد فقط العدد المطلوب (Take)
    }

    [Fact]
    public async Task Handle_WhenSkipIsZero_ComputesStatsCorrectly()
    {
        // Arrange
        var query = new GetActivityLogsQuery(Skip: 0, Take: 5);
        var today = DateTimeOffset.UtcNow;

        var fakeLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                UserEmail = "system",
                Action = "DELETE",
                TableName = "Quiz",
                CreatedAt = today
            },
            new()
            {
                Id = 2,
                UserEmail = "system",
                Action = "UPDATE",
                TableName = "Lesson",
                CreatedAt = today
            }
        };

        _auditLogRepository.ListAsync(Arg.Any<ActivityLogsFilterSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Stats.Should().NotBeNull();
        result.Value.Stats!.TotalEvents.Should().Be(2);
        result.Value.Stats.TodayEvents.Should().Be(2);
        result.Value.Stats.Alerts.Should().Be(1); // لأن الأول فيه DELETE
    }

    [Fact]
    public async Task Handle_WhenSkipIsGreaterThanZero_ReturnsNullStats()
    {
        // Arrange
        var query = new GetActivityLogsQuery(Skip: 10, Take: 5);
        _auditLogRepository.ListAsync(Arg.Any<ActivityLogsFilterSpec>(), Arg.Any<CancellationToken>())
            .Returns(new List<AuditLog>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Stats.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithJsonPayload_ExtractsDetailsCorrectly()
    {
        // Arrange
        var query = new GetActivityLogsQuery(Skip: 0, Take: 1);
        var oldValuesJson = JsonSerializer.Serialize(new { Title = "كورس السي شارب" });

        var fakeLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                UserEmail = "system",
                Action = "DELETE",
                TableName = "lesson",
                OldValues = oldValuesJson
            }
        };

        _auditLogRepository.ListAsync(Arg.Any<ActivityLogsFilterSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeLogs);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Events[0].Detail.Should().Be("كورس السي شارب");
    }
}