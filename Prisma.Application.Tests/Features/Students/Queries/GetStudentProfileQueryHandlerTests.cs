using FluentAssertions;
using NSubstitute;
using Ardalis.Specification;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Prisma.Application.Tests.Features.Students.Queries;

public class GetStudentProfileQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Student, Guid> _studentRepo = Substitute.For<IRepository<Student, Guid>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly GetStudentProfileQueryHandler _sut;

    public GetStudentProfileQueryHandlerTests()
    {
        _unitOfWork.GetOrCreateRepository<Student, Guid>().Returns(_studentRepo);
        _sut = new GetStudentProfileQueryHandler(_unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetStudentProfileQuery();
        _currentUserService.UserId.Returns((Guid?)null); // محاكاة عدم تسجيل الدخول

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        result.Errors.Should().Contain("User is not authenticated.");
    }

    [Fact]
    public async Task Handle_WhenStudentDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetStudentProfileQuery();
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // محاكاة إرجاع null عند البحث عن الطالب باستخدام الـ Specification
        _studentRepo.FirstOrDefaultAsync(Arg.Any<StudentWithProfileSpec>(), Arg.Any<CancellationToken>())
            .Returns((Student)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenStudentExists_ReturnsMappedStudentProfileDto()
    {
        // Arrange
        var query = new GetStudentProfileQuery();
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        var fakeStudent = new Student
        {
            Id = currentUserId,
            FirstName = "أحمد",
            SecondName = "محمود",
            ThirdName = "علي",
            LastName = "سعيد",
            PhoneNumber = "01012345678",
            Email = "student@prisma.com",
            AcademicYearId = 3,
            ParentPhoneNumber = "01112345678"
        };

        _studentRepo.FirstOrDefaultAsync(Arg.Any<StudentWithProfileSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeStudent);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التحقق من صحة الـ Mapping لجميع الحقول المرسلة في الـ Dto
        result.Value.FirstName.Should().Be("أحمد");
        result.Value.SecondName.Should().Be("محمود");
        result.Value.ThirdName.Should().Be("علي");
        result.Value.LastName.Should().Be("سعيد");
        result.Value.Mobile.Should().Be("01012345678");
        result.Value.Email.Should().Be("student@prisma.com");
        result.Value.Grade.Should().Be(3);
        result.Value.ParentMobile.Should().Be("01112345678");
    }

    [Fact]
    public async Task Handle_WhenStudentHasNullProperties_ReturnsDtoWithEmptyStrings()
    {
        // Arrange
        var query = new GetStudentProfileQuery();
        var currentUserId = Guid.NewGuid();
        _currentUserService.UserId.Returns(currentUserId);

        // التأكد من معالجة قيم الـ null (توقع تحويلها إلى نصوص فارغة كما هو مبرمج بالـ Handler)
        var fakeStudentWithNulls = new Student
        {
            Id = currentUserId,
            FirstName = null,
            SecondName = null,
            ThirdName = null,
            LastName = null,
            PhoneNumber = null,
            Email = null,
            AcademicYearId = null,
            ParentPhoneNumber = null
        };

        _studentRepo.FirstOrDefaultAsync(Arg.Any<StudentWithProfileSpec>(), Arg.Any<CancellationToken>())
            .Returns(fakeStudentWithNulls);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التحقق من تفعيل معاملات الـ null coalescing (?? string.Empty) داخل الكود الخاص بك
        result.Value.FirstName.Should().Be(string.Empty);
        result.Value.SecondName.Should().Be(string.Empty);
        result.Value.ThirdName.Should().Be(string.Empty);
        result.Value.LastName.Should().Be(string.Empty);
        result.Value.Mobile.Should().Be(string.Empty);
        result.Value.Email.Should().Be(string.Empty);
        result.Value.Grade.Should().Be(0); // الـ int? يتحول لـ 0
        result.Value.ParentMobile.Should().Be(string.Empty);
    }
}