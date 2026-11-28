using Ardalis.Result;
using Prisma.Domain.Aggregates.FileObjectAggregate;
using Prisma.Domain.Aggregates.ObjectLibraryAggregate;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.ValueObjects.UserDomain;

namespace Prisma.Domain.Entities.UserAggregate;

public class Teacher : User
{
    public string Subject { get; set; } = string.Empty;
    public string? TeacherAvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsFeatured { get; set; }

    // ── Public profile (edited by the teacher) ──
    public int? YearsOfExperience { get; set; }
    public ProfileColor Color { get; set; } = ProfileColor.Ocean;
    public ContactLinks Links { get; set; } = new();
    public ProfileVisibility Visibility { get; set; } = new();

    // Short pinned message at the top of the public page. After AnnouncementEndsAt (optional) it
    // stops showing; the text stays so the teacher can edit or reuse it.
    public string? AnnouncementText { get; set; }
    public DateTimeOffset? AnnouncementEndsAt { get; set; }

    // Profile picture: one file in the teacher's ObjectStorage. Its ObjectKey plus the storage's
    // BucketName give the download link. The browser uploads straight to storage; FileStatus is not used.
    public Guid? ProfileImageFileId { get; set; }
    public FileObject? ProfileImageFile { get; set; }

    //public ICollection<Student> Students { get; set; } = new List<Student>();
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    public string? SuspensionReason { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<AcademicYearTeacher> AcademicYears { get; set; } = [];
    public ICollection<Assistant> Assistants { get; set; } = [];
    public TeacherLandingSettings? TeacherLandingSettings { get; set; }
    public TeacherPreferences Preferences { get; set; } = null!;
    protected override IReadOnlySet<string> AllowedTours => TourKeys.ForTeacher.All;

    // public VideoLibrary? VideoLibrary { get; private set; }
    // public int? VideoLibraryId { get; private set; }

    // public TenantSetting? TenantSetting { get; set; }
    public ICollection<TeacherStudent> TeacherStudents { get; set; } = [];
    public ICollection<Quiz> Quizzes { get; set; } = [];

    //public ICollection<Student> Students { get; set; } = new List<Student>();

    //TODO
    // public ICollection<Subject> Subjects { get; set; } = [];

    public static Result<Teacher> Create(
        string firstName,
        string secondName,
        string rawEmail,
        string rawPhone,
        string subject,
        string thirdName,
        string lastName
    )
    {
        // 1. Instantiating / converting to Value Objects
        var fullNameResult = FullName.Create(firstName, secondName, thirdName, lastName);
        var emailAddressResult = ValueObjects.UserDomain.EmailAddress.Create(rawEmail);
        var mobilePhoneNumberResult = ValueObjects.UserDomain.PhoneNumber.Create(rawPhone);

        // 2. combine validation failures
        var validationErrors = new List<ValidationError>();

        if (!fullNameResult.IsSuccess)
        {
            validationErrors.AddRange(fullNameResult.ValidationErrors);
        }

        if (!emailAddressResult.IsSuccess)
        {
            validationErrors.AddRange(emailAddressResult.ValidationErrors);
        }

        if (!mobilePhoneNumberResult.IsSuccess)
        {
            validationErrors.AddRange(mobilePhoneNumberResult.ValidationErrors);
        }

        if (validationErrors.Count > 0)
        {
            return Result.Invalid(validationErrors);
        }

        // 3. Initializing Aggregate Root
        var teacher = new Teacher()
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            SecondName = secondName,
            ThirdName = thirdName,
            LastName = lastName,
            FullName = fullNameResult.Value,
            EmailAddress = emailAddressResult.Value,
            MobilePhoneNumber = mobilePhoneNumberResult.Value,
            Subject = subject,

            // Syncing base IdentityUser fields
            UserName = emailAddressResult.Value.Value,
            Email = emailAddressResult.Value.Value,
            PhoneNumber = mobilePhoneNumberResult.Value.Value,
            IsBlocked = false,
            IsOnline = false,
        };
        teacher.AddDomainEvent(new TeacherCreatedEvent(teacher.Id));
        return Result.Success(teacher);
    }

    /// <summary>Applies the fields the teacher edits on the public profile page.</summary>
    public Result UpdateProfile(
        string firstName,
        string secondName,
        string? thirdName,
        string? lastName,
        string subject,
        string? bio,
        int? yearsOfExperience,
        ProfileColor color,
        ContactLinks links,
        ProfileVisibility visibility
    )
    {
        var fullNameResult = FullName.Create(firstName, secondName, thirdName, lastName);
        if (!fullNameResult.IsSuccess)
        {
            return Result.Invalid(fullNameResult.ValidationErrors);
        }

        var fullName = fullNameResult.Value;
        FirstName = fullName.FirstName;
        SecondName = fullName.SecondName;
        ThirdName = fullName.ThirdName;
        LastName = fullName.LastName;
        FullName = fullName;

        Subject = subject;
        Bio = bio;
        YearsOfExperience = yearsOfExperience;
        Color = color;
        Links = links;
        Visibility = visibility;

        return Result.Success();
    }

    /// <summary>Sets or clears the pinned announcement. An end date means nothing without a text.</summary>
    public void SetAnnouncement(string? text, DateTimeOffset? endsAt)
    {
        var trimmed = string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        AnnouncementText = trimmed;
        AnnouncementEndsAt = trimmed is null ? null : endsAt;
    }

    /// <summary>Points the profile at a newly created image file (uploaded by the browser via a presigned link).</summary>
    public void SetProfileImage(FileObject file)
    {
        ArgumentNullException.ThrowIfNull(file);

        ProfileImageFile = file;
        ProfileImageFileId = file.Id;
    }

    public void RemoveProfileImage()
    {
        ProfileImageFile = null;
        ProfileImageFileId = null;
    }

    // public void AttachVideoLibrary(VideoLibrary library)
    // {
    //     ArgumentNullException.ThrowIfNull(library);
    //     VideoLibraryId = library.Id;
    //     VideoLibrary = library;
    // }
}
