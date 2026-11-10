using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Lessons.Queries.GetLessonEditorDetails;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Tests.Features.Lessons.Queries;

public class GetLessonEditorDetailsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IRepository<Lesson, int> _lessonRepo = Substitute.For<
        IRepository<Lesson, int>
    >();

    private readonly IRepository<AcademicYear, int> _academicYearRepo = Substitute.For<
        IRepository<AcademicYear, int>
    >();

    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly GetLessonEditorDetailsQueryHandler _sut;
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ICurrentUserService _currentUserService =
        Substitute.For<ICurrentUserService>();

    public GetLessonEditorDetailsQueryHandlerTests()
    {
        // إعداد الـ Repositories داخل الـ Unit of Work
        _unitOfWork.GetOrCreateRepository<Lesson, int>().Returns(_lessonRepo);
        _unitOfWork.GetOrCreateRepository<AcademicYear, int>().Returns(_academicYearRepo);

        _storageService.DefaultBucketName.Returns("prisma-bucket");

        _sut = new GetLessonEditorDetailsQueryHandler(
            _unitOfWork,
            _storageService,
            _currentUserService,
            _identityService
        );
    }

    private Guid SetupAuthenticatedTeacher()
    {
        var teacherId = Guid.NewGuid();
        _currentUserService.UserId.Returns(teacherId);
        _identityService
            .FindByIdAsync(teacherId, Arg.Any<CancellationToken>())
            .Returns(new Prisma.Domain.Entities.UserAggregate.Teacher { Id = teacherId });

        return teacherId;
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        var query = new GetLessonEditorDetailsQuery(1);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Unauthorized);
        await _lessonRepo
            .DidNotReceive()
            .FirstOrDefaultAsync(
                Arg.Any<GetLessonEditorDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenLessonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        SetupAuthenticatedTeacher();
        var query = new GetLessonEditorDetailsQuery(1);

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<GetLessonEditorDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns((LessonEditorDetailsProjection?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotLessonOwner_ReturnsForbidden()
    {
        // Arrange
        SetupAuthenticatedTeacher();
        var lessonId = 1;
        var query = new GetLessonEditorDetailsQuery(lessonId);

        var fakeProjection = new LessonEditorDetailsProjection
        {
            Id = lessonId,
            TeacherId = Guid.NewGuid(),
            Money = Money.Create(100m, "EGP").Value,
            ValidityDuration = TimeDuration.Zero,
        };

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<GetLessonEditorDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeProjection);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
        result.Errors.Should().Contain("You are not the owner of this lesson.");
    }

    [Fact]
    public async Task Handle_WhenLessonExists_ReturnsMappedLessonEditorResponseDto()
    {
        // Arrange
        var teacherId = SetupAuthenticatedTeacher();
        var lessonId = 1;
        var query = new GetLessonEditorDetailsQuery(lessonId);

        // 1. بناء بيانات الدرس الوهمية (كـ projection مش entity كاملة)
        var fakeProjection = new LessonEditorDetailsProjection
        {
            Id = lessonId,
            PublicId = Guid.NewGuid(),
            Title = "درس النحو: الفاعل",
            Description = "شرح مفصل لدرس الفاعل وعلامات إعرابه",
            Money = Money.Create(100.00m, "EGP").Value,
            PrerequisiteId = 5,
            ImageThumbnailUrl = "lesson-thumb.jpg",
            TeacherId = teacherId,
            ValidityDuration = TimeDuration.FromSeconds(86400).Value,
            Outcomes = new List<string>
            {
                "أن يتعرف الطالب على الفاعل",
                "أن يعرب الفاعل إعراباً صحيحاً",
            },
            Sections = new List<EditorSectionProjection>
            {
                new()
                {
                    Title = "فيديو الشرح",
                    ContentURL = "video1.mp4",
                    SortOrder = 1,
                },
                new()
                {
                    Title = "مقدمة الدرس",
                    ContentURL = "intro.mp4",
                    SortOrder = 0,
                }, // الترتيب الصغير يظهر أولاً بالـ OrderBy
            },
            HasAssignment = true,
            AssignmentTitle = "واجب درس الفاعل.pdf",
            AssignmentDueDate = DateTimeOffset.UtcNow.AddDays(3),
            AcademicYearIds = new List<int> { 2 }, // الصف الثاني الإعدادي كمثال
        };

        // 2. بناء الـ Prerequisites الوهمية
        var fakePrerequisitesOptions = new List<Lesson>
        {
            new() { Id = 5, Title = "درس الجملة الفعلية" },
        };

        // 3. بناء جميع المراحل الدراسية الوهمية
        var fakeAllAcademicYears = new List<AcademicYear>
        {
            new() { Id = 1, Title = "الصف الأول الإعدادي" },
            new() { Id = 2, Title = "الصف الثاني الإعدادي" },
        };

        // إعداد الـ Mocks للـ Repositories بناءً على الـ Specifications والتوقيعات المستخدمة
        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<GetLessonEditorDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeProjection);

        _lessonRepo
            .ListAsync(
                Arg.Any<LessonPrerequisiteOptionsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakePrerequisitesOptions);

        _academicYearRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(fakeAllAcademicYears);

        // إعداد الـ Mock الخاص بالـ Storage Service
        _storageService
            .GetDownloadUrlAsync("prisma-bucket", "lesson-thumb.jpg")
            .Returns("https://cdn.prisma.com/lesson-thumb.jpg");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // التأكد من صحة الحقول الأساسية وعمل الـ Mapping لها
        result.Value.Id.Should().Be(lessonId);
        result.Value.PublicId.Should().Be(fakeProjection.PublicId);
        result.Value.Title.Should().Be("درس النحو: الفاعل");
        result.Value.Description.Should().Be("شرح مفصل لدرس الفاعل وعلامات إعرابه");
        result.Value.Money.Amount.Should().Be(100.00m);
        result.Value.Money.Currency.Should().Be("EGP");
        result.Value.PrerequisiteLessonId.Should().Be(5);
        result.Value.ImageUrl.Should().Be("https://cdn.prisma.com/lesson-thumb.jpg");
        result.Value.ValidityDuration.Seconds.Should().Be(86400);

        // التأكد من ترتيب الـ Chapters بناءً على الـ SortOrder (الـ مقدمة أولاً لأن الـ SortOrder لها 0)
        result.Value.Chapters.Should().HaveCount(2);
        result.Value.Chapters[0].Name.Should().Be("مقدمة الدرس");
        result.Value.Chapters[0].VideoFileName.Should().Be("intro.mp4");
        result.Value.Chapters[1].Name.Should().Be("فيديو الشرح");

        // التأكد من بيانات الـ Assignment
        result.Value.AssignmentEnabled.Should().BeTrue();
        result.Value.AssignmentFileName.Should().Be("واجب درس الفاعل.pdf");
        result.Value.AssignmentDueDate.Should().Be(fakeProjection.AssignmentDueDate);

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

    [Fact]
    public async Task Handle_WhenLessonHasNoAssignment_ReturnsAssignmentDisabled()
    {
        // Arrange
        var teacherId = SetupAuthenticatedTeacher();
        var lessonId = 2;
        var query = new GetLessonEditorDetailsQuery(lessonId);

        var fakeProjection = new LessonEditorDetailsProjection
        {
            Id = lessonId,
            Title = "درس بدون واجب",
            TeacherId = teacherId,
            Money = Money.Zero().Value,
            ValidityDuration = TimeDuration.Zero,
            HasAssignment = false,
            AssignmentTitle = null,
            AssignmentDueDate = null,
            Sections = new List<EditorSectionProjection>(),
        };

        _lessonRepo
            .FirstOrDefaultAsync(
                Arg.Any<GetLessonEditorDetailsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(fakeProjection);

        _lessonRepo
            .ListAsync(
                Arg.Any<LessonPrerequisiteOptionsSpecification>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new List<Lesson>());

        _academicYearRepo.ListAsync(Arg.Any<CancellationToken>()).Returns(new List<AcademicYear>());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AssignmentEnabled.Should().BeFalse();
        result.Value.AssignmentFileName.Should().BeNull();
        result.Value.AssignmentDueDate.Should().BeNull();
        result.Value.ImageUrl.Should().Be(string.Empty);
    }
}
