using FluentAssertions;
using NSubstitute;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Application.Features.Lessons.Queries.GetLessonFormOptions;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Prisma.Application.Abstractions.Services;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonFormOptionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();

    private readonly IRepository<AcademicYear, int>
        _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();

    private readonly GetLessonFormOptionsQueryHandler _sut;
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    public GetLessonFormOptionsQueryHandlerTests()
    {
        // إعداد الـ Repositories داخل الـ Unit of Work الوهمي
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        _sut = new GetLessonFormOptionsQueryHandler(_unitOfWork, _identityService, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenOptionsExist_ReturnsMappedLessonFormOptionsResponseDto()
    {
        // Arrange
        var query = new GetLessonFormOptionsQuery();

        // بناء دروس وهمية لتكون خيارات للمتطلبات السابقة (Prerequisites)
        var fakeLessons = new List<Lesson>
        {
            new() { Id = 10, Title = "درس القراءة: الحرية" }, new() { Id = 11, Title = "درس النحو: المبتدأ والخبر" }
        };

        // بناء مراحل دراسية وهمية
        var fakeAcademicYears = new List<AcademicYear>
        {
            new() { Id = 1, Title = "الصف الأول الإعدادي" }, new() { Id = 2, Title = "الصف الثاني الإعدادي" }
        };

        // عمل Mock للـ ListAsync لكل Repository لترجع البيانات المجهزة
        _lessonRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(fakeLessons);
        _academicYearRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(fakeAcademicYears);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التحقق من صحة الخيارات الخاصة بالمتطلبات السابقة للدروس (PrerequisitesOptions)
        result.Value.PrerequisitesOptions.Should().HaveCount(2);
        result.Value.PrerequisitesOptions[0].Id.Should().Be(10);
        result.Value.PrerequisitesOptions[0].Name.Should().Be("درس القراءة: الحرية");
        result.Value.PrerequisitesOptions[1].Id.Should().Be(11);
        result.Value.PrerequisitesOptions[1].Name.Should().Be("درس النحو: المبتدأ والخبر");

        // التحقق من صحة الخيارات الخاصة بالمراحل الدراسية (AllAcademicYearsOptions)
        result.Value.AllAcademicYearsOptions.Should().HaveCount(2);
        result.Value.AllAcademicYearsOptions[0].Id.Should().Be(1);
        result.Value.AllAcademicYearsOptions[0].Name.Should().Be("الصف الأول الإعدادي");
        result.Value.AllAcademicYearsOptions[1].Id.Should().Be(2);
        result.Value.AllAcademicYearsOptions[1].Name.Should().Be("الصف الثاني الإعدادي");
    }

    [Fact]
    public async Task Handle_WhenNoOptionsExist_ReturnsEmptyOptionsLists()
    {
        // Arrange
        var query = new GetLessonFormOptionsQuery();

        // محاكاة قواعد البيانات الفارغة بإرجاع قوائم فارغة
        _lessonRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<Lesson>());
        _academicYearRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<AcademicYear>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التأكد من أن القوائم فارغة تماماً وليست Null لضمان عدم حدوث مشاكل في فرونت إند
        result.Value.PrerequisitesOptions.Should().BeEmpty();
        result.Value.AllAcademicYearsOptions.Should().BeEmpty();
    }
}