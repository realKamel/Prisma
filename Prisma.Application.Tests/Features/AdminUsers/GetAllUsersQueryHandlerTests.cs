using FluentAssertions;
using NSubstitute;
using Prisma.Application.Features.Users.Queries.GetAllUsers;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Tests.Features.AdminUsers;

public class GetAllUsersQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<IRepository<Student, Guid>>();

    private readonly IRepository<global::Prisma.Domain.Entities.UserAggregate.Teacher, Guid> _teacherRepo =
        Substitute.For<IRepository<global::Prisma.Domain.Entities.UserAggregate.Teacher, Guid>>();

    private readonly IRepository<Assistant, Guid> _assistantRepo = Substitute.For<IRepository<Assistant, Guid>>();

    private readonly IRepository<Domain.Entities.UserAggregate.Admin, Guid> _adminRepo =
        Substitute.For<IRepository<Domain.Entities.UserAggregate.Admin, Guid>>();

    private readonly GetAllUsersQueryHandler _sut;

    public GetAllUsersQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _unitOfWork.GetOrCreateRepository<global::Prisma.Domain.Entities.UserAggregate.Teacher, Guid>()
            .Returns(_teacherRepo);
        _unitOfWork.GetOrCreateRepository<Assistant, Guid>().Returns(_assistantRepo);
        _unitOfWork.GetOrCreateRepository<Domain.Entities.UserAggregate.Admin, Guid>()
            .Returns(_adminRepo);

        _sut = new GetAllUsersQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_MergesAllRolesAndExcludesSoftDeletedUsers()
    {
        // Arrange
        var activeStudent = new Student
        {
            Id = Guid.CreateVersion7(),
            FirstName = "أحمد",
            LastName = "علي",
            Email = "ahmed@test.com",
            IsBlocked = false,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
        };
        var deletedStudent = new Student
        {
            Id = Guid.CreateVersion7(),
            FirstName = "محذوف",
            LastName = "مستخدم",
            Email = "deleted@test.com",
            IsDeleted = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
        };
        var teacher = new global::Prisma.Domain.Entities.UserAggregate.Teacher
        {
            Id = Guid.CreateVersion7(),
            FirstName = "سارة",
            LastName = "خالد",
            Email = "sara@test.com",
            IsBlocked = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        };

        _studentRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Student> { activeStudent, deletedStudent });
        _teacherRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<global::Prisma.Domain.Entities.UserAggregate.Teacher> { teacher });
        _assistantRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<Assistant>());
        _adminRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<global::Prisma.Domain.Entities.UserAggregate.Admin>());

        // Act
        var result = await _sut.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2); // soft-deleted student excluded
        result.Value.Should().NotContain(u => u.Email == "deleted@test.com");

        var teacherDto = result.Value.Single(u => u.Email == "sara@test.com");
        teacherDto.Role.Should().Be("Teacher");
        teacherDto.Active.Should().BeFalse(); // IsBlocked == true

        var studentDto = result.Value.Single(u => u.Email == "ahmed@test.com");
        studentDto.Role.Should().Be("Student");
        studentDto.Active.Should().BeTrue();
        studentDto.Name.Should().Be("أحمد علي");
    }

    [Fact]
    public async Task Handle_OrdersUsersByJoinedDateDescending()
    {
        // Arrange
        var older = new global::Prisma.Domain.Entities.UserAggregate.Admin
        {
            Id = Guid.CreateVersion7(),
            FirstName = "قديم",
            LastName = "مدير",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };
        var newer = new global::Prisma.Domain.Entities.UserAggregate.Admin
        {
            Id = Guid.CreateVersion7(),
            FirstName = "جديد",
            LastName = "مدير",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _studentRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<Student>());
        _teacherRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<global::Prisma.Domain.Entities.UserAggregate.Teacher>());
        _assistantRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<Assistant>());
        _adminRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<global::Prisma.Domain.Entities.UserAggregate.Admin> { older, newer });

        // Act
        var result = await _sut.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        result.Value.First().Name.Should().Be("جديد مدير");
    }
}