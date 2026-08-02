using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Students.Commands.UpdateStudentProfileCommand;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Students.Commands;

public class UpdateStudentProfileCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<IRepository<Student, Guid>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private readonly UserManager<User> _userManager = Substitute.For<UserManager<User>>(
        Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);

    private readonly UpdateStudentProfileCommandHandler _sut;

    public UpdateStudentProfileCommandHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _sut = new UpdateStudentProfileCommandHandler(_unitOfWork, _currentUserService, _userManager);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        var command = new UpdateStudentProfileCommand("احمد", "محمد", "علي", "مصطفى", "01012345678", 1, "01212345678");
        _currentUserService.UserId.Returns((Guid?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenStudentDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var command = new UpdateStudentProfileCommand("احمد", "محمد", "علي", "مصطفى", "01012345678", 1, "01212345678");
        _currentUserService.UserId.Returns(currentUserId);

        _studentRepo.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>()).Returns((Student)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenStudentExists_UpdatesProfileAndReturnsSuccess()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        // إدخال نصوص تحتوي على مسافات زائدة لاختبار الـ Trim() داخل الهاندلر
        var command = new UpdateStudentProfileCommand("  احمد  ", " محمد ", " علي ", " مصطفى ", " 01012345678 ", 3, " 01212345678 ");
        _currentUserService.UserId.Returns(currentUserId);

        var fakeStudent = new Student { Id = currentUserId };
        _studentRepo.GetByIdAsync(currentUserId, Arg.Any<CancellationToken>()).Returns(fakeStudent);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        result.GetResultMessage().Should().Be("Success"); // يتوافق تماماً مع الـ Base Result الافتراضية عند النجاح لديك

        // التأكد من تطبيق الـ Trim وتحديث البيانات على الكيان الأصلي بنجاح
        fakeStudent.FirstName.Should().Be("احمد");
        fakeStudent.SecondName.Should().Be("محمد");
        fakeStudent.ThirdName.Should().Be("علي");
        fakeStudent.LastName.Should().Be("مصطفى");
        fakeStudent.PhoneNumber.Should().Be("01012345678");
        fakeStudent.AcademicYearId.Should().Be(3);
        fakeStudent.ParentPhoneNumber.Should().Be("01212345678");

        // التأكد من استدعاء مستودع البيانات وحفظ التغييرات في الـ Database
        _studentRepo.Received(1).Update(fakeStudent);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}