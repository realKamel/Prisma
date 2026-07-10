using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Xunit;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonEditorDetailsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();
    private readonly IRepository<AcademicYear, int> _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();
    private readonly GetLessonEditorDetailsQueryHandler _sut;

    public GetLessonEditorDetailsQueryHandlerTests()
    {
        // إعداد الـ Repositories داخل الـ Unit of Work
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        // إعداد الـ Configuration للـ Storage Section
        var configSection = Substitute.For<IConfigurationSection>();
        configSection["BucketName"].Returns("prisma-bucket");
        _configuration.GetSection("Storage").Returns(configSection);

        _sut = new GetLessonEditorDetailsQueryHandler(_unitOfWork, _storageService, _configuration);
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonEditorDetailsQuery(1);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<GetLessonEditorDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonEditorResponseDto()
    {
        // Arrange
        var lessonId = 1;
        var query = new GetLessonEditorDetailsQuery(lessonId);

        // 1. بناء بيانات الدرس الوهمية
        var fakeLesson = new Lesson
        {
            Id = lessonId,
            Title = "درس النحو: الفاعل",
            Description = "شرح مفصل لدرس الفاعل وعلامات إعرابه",
            Price = 100.00m,
            PrerequisiteId = 5,
            ImageThumbnailUrl = "lesson-thumb.jpg",
            Outcomes = new List<string> { "أن يتعرف الطالب على الفاعل", "أن يعرب الفاعل إعراباً صحيحاً" },
            Sections = new List<Section>
            {
                new() { Title = "فيديو الشرح", ContentURL = "video1.mp4", SortOrder = 1 },
                new() { Title = "مقدمة الدرس", ContentURL = "intro.mp4", SortOrder = 0 } // الترتيب الصغير يظهر أولاً بالـ OrderBy
            },
            Assignment = new Assignment
            {
                Title = "واجب درس الفاعل.pdf",
                DueDate = DateTimeOffset.UtcNow.AddDays(3)
            },
            AcademicYears = new List<AcademicYearLesson>
            {
                new() { AcademicYearId = 2 } // الصف الثاني الإعدادي كمثال
            }
        };

        // 2. بناء الـ Prerequisites الوهمية
        var fakePrerequisitesOptions = new List<Lesson>
        {
            new() { Id = 5, Title = "درس الجملة الفعلية" }
        };

        // 3. بناء جميع المراحل الدراسية الوهمية
        var fakeAllAcademicYears = new List<AcademicYear>
        {
            new() { Id = 1, Title = "الصف الأول الإعدادي" },
            new() { Id = 2, Title = "الصف الثاني الإعدادي" }
        };

        // إعداد الـ Mocks للـ Repositories بناءً على الـ Specifications والتوقيعات المستخدمة
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<GetLessonEditorDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        _lessonRepo.ListAsync(Arg.Any<LessonPrerequisiteOptionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakePrerequisitesOptions);

        _academicYearRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(fakeAllAcademicYears);

        // إعداد الـ Mock الخاص بالـ Storage Service
        _storageService.GetPublicUrl("prisma-bucket", "lesson-thumb.jpg").Returns("https://cdn.prisma.com/lesson-thumb.jpg");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // التأكد من صحة الحقول الأساسية وعمل الـ Mapping لها
        result.Data.Id.Should().Be(lessonId);
        result.Data.Title.Should().Be("درس النحو: الفاعل");
        result.Data.Description.Should().Be("شرح مفصل لدرس الفاعل وعلامات إعرابه");
        result.Data.Price.Should().Be(100.00m);
        result.Data.PrerequisiteLessonId.Should().Be(5);
        result.Data.ImageUrl.Should().Be("https://cdn.prisma.com/lesson-thumb.jpg");

        // التأكد من ترتيب الـ Chapters بناءً على الـ SortOrder (الـ مقدمة أولاً لأن الـ SortOrder لها 0)
        result.Data.Chapters.Should().HaveCount(2);
        result.Data.Chapters[0].Name.Should().Be("مقدمة الدرس");
        result.Data.Chapters[0].VideoFileName.Should().Be("intro.mp4");
        result.Data.Chapters[1].Name.Should().Be("فيديو الشرح");

        // التأكد من بيانات الـ Assignment
        result.Data.AssignmentEnabled.Should().BeTrue();
        result.Data.AssignmentFileName.Should().Be("واجب درس الفاعل.pdf");
        result.Data.AssignmentDueDate.Should().Be(fakeLesson.Assignment.DueDate);

        // التأكد من الـ Outcomes
        result.Data.Outcomes.Should().HaveCount(2).And.Contain("أن يتعرف الطالب على الفاعل");

        // التأكد من ربط المراحل المختارة والخيارات المتاحة
        result.Data.SelectedAcademicYears.Should().ContainSingle().Which.Should().Be(2);

        result.Data.PrerequisitesOptions.Should().HaveCount(1);
        result.Data.PrerequisitesOptions[0].Name.Should().Be("درس الجملة الفعلية");
        result.Data.PrerequisitesOptions[0].Id.Should().Be(5);

        result.Data.AllAcademicYearsOptions.Should().HaveCount(2);
        result.Data.AllAcademicYearsOptions[0].Name.Should().Be("الصف الأول الإعدادي");
    }
}