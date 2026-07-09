using FluentAssertions;
using NSubstitute;
using Ardalis.Specification;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Students.Queries.GetStudentProfileQuery;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
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
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("User is not authenticated.");
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
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
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
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // التحقق من صحة الـ Mapping لجميع الحقول المرسلة في الـ Dto
        result.Data.FirstName.Should().Be("أحمد");
        result.Data.SecondName.Should().Be("محمود");
        result.Data.ThirdName.Should().Be("علي");
        result.Data.LastName.Should().Be("سعيد");
        result.Data.Mobile.Should().Be("01012345678");
        result.Data.Email.Should().Be("student@prisma.com");
        result.Data.Grade.Should().Be(3);
        result.Data.ParentMobile.Should().Be("01112345678");
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
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // التحقق من تفعيل معاملات الـ null coalescing (?? string.Empty) داخل الكود الخاص بك
        result.Data.FirstName.Should().Be(string.Empty);
        result.Data.SecondName.Should().Be(string.Empty);
        result.Data.ThirdName.Should().Be(string.Empty);
        result.Data.LastName.Should().Be(string.Empty);
        result.Data.Mobile.Should().Be(string.Empty);
        result.Data.Email.Should().Be(string.Empty);
        result.Data.Grade.Should().Be(0); // الـ int? يتحول لـ 0
        result.Data.ParentMobile.Should().Be(string.Empty);
    }
}