using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;


namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonEditorDetailsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<IRepository<Lesson, int>>();

    private readonly IRepository<AcademicYear, int>
        _academicYearRepo = Substitute.For<IRepository<AcademicYear, int>>();

    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly GetLessonEditorDetailsQueryHandler _sut;

    public GetLessonEditorDetailsQueryHandlerTests()
    {
        // إعداد الـ Repositories داخل الـ Unit of Work
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        // إعداد الـ Configuration للـ Storage Section
        var configSection = Substitute.For<IConfigurationSection>();
        configSection["BucketName"].Returns("prisma-bucket");
        _storageService.DefaultBucketName.Returns("prisma-bucket");

        _sut = new GetLessonEditorDetailsQueryHandler(_unitOfWork, _storageService);
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetLessonEditorDetailsQuery(1);

        _lessonRepo.FirstOrDefaultAsync(Arg.Any<GetLessonEditorDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Lesson)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
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
                new()
                {
                    Title = "مقدمة الدرس", ContentURL = "intro.mp4", SortOrder = 0
                } // الترتيب الصغير يظهر أولاً بالـ OrderBy
            },
            Assignment = new Assignment { Title = "واجب درس الفاعل.pdf", DueDate = DateTimeOffset.UtcNow.AddDays(3) },
            AcademicYears = new List<AcademicYearLesson>
            {
                new() { AcademicYearId = 2 } // الصف الثاني الإعدادي كمثال
            }
        };

        // 2. بناء الـ Prerequisites الوهمية
        var fakePrerequisitesOptions = new List<Lesson> { new() { Id = 5, Title = "درس الجملة الفعلية" } };

        // 3. بناء جميع المراحل الدراسية الوهمية
        var fakeAllAcademicYears = new List<AcademicYear>
        {
            new() { Id = 1, Title = "الصف الأول الإعدادي" }, new() { Id = 2, Title = "الصف الثاني الإعدادي" }
        };

        // إعداد الـ Mocks للـ Repositories بناءً على الـ Specifications والتوقيعات المستخدمة
        _lessonRepo.FirstOrDefaultAsync(Arg.Any<GetLessonEditorDetailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakeLesson);

        _lessonRepo.ListAsync(Arg.Any<LessonPrerequisiteOptionsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(fakePrerequisitesOptions);

        _academicYearRepo.ListAsync(Arg.Any<CancellationToken>())
            .Returns(fakeAllAcademicYears);

        // إعداد الـ Mock الخاص بالـ Storage Service
        _storageService.GetDownloadUrlAsync("prisma-bucket", "lesson-thumb.jpg")
            .Returns("https://cdn.prisma.com/lesson-thumb.jpg");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // التأكد من صحة الحقول الأساسية وعمل الـ Mapping لها
        result.Value.Id.Should().Be(lessonId);
        result.Value.Title.Should().Be("درس النحو: الفاعل");
        result.Value.Description.Should().Be("شرح مفصل لدرس الفاعل وعلامات إعرابه");
        result.Value.Price.Should().Be(100.00m);
        result.Value.PrerequisiteLessonId.Should().Be(5);
        result.Value.ImageUrl.Should().Be("https://cdn.prisma.com/lesson-thumb.jpg");

        // التأكد من ترتيب الـ Chapters بناءً على الـ SortOrder (الـ مقدمة أولاً لأن الـ SortOrder لها 0)
        result.Value.Chapters.Should().HaveCount(2);
        result.Value.Chapters[0].Name.Should().Be("مقدمة الدرس");
        result.Value.Chapters[0].VideoFileName.Should().Be("intro.mp4");
        result.Value.Chapters[1].Name.Should().Be("فيديو الشرح");

        // التأكد من بيانات الـ Assignment
        result.Value.AssignmentEnabled.Should().BeTrue();
        result.Value.AssignmentFileName.Should().Be("واجب درس الفاعل.pdf");
        result.Value.AssignmentDueDate.Should().Be(fakeLesson.Assignment.DueDate);

        // التأكد من الـ Outcomes
        result.Value.Outcomes.Should().HaveCount(2).And.Contain("أن يتعرف الطالب على الفاعل");

        // التأكد من ربط المراحل المختارة والخيارات المتاحة
        result.Value.SelectedAcademicYears.Should().ContainSingle().Which.Should().Be(2);

        result.Value.PrerequisitesOptions.Should().HaveCount(1);
        result.Value.PrerequisitesOptions[0].Name.Should().Be("درس الجملة الفعلية");
        result.Value.PrerequisitesOptions[0].Id.Should().Be(5);

        result.Value.AllAcademicYearsOptions.Should().HaveCount(2);
        result.Value.AllAcademicYearsOptions[0].Name.Should().Be("الصف الأول الإعدادي");
    }
}