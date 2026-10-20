using Ardalis.Result;
using Prisma.Domain.DomainEvents;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.UserAggregate;

public class Teacher : User
{
    public string Subject { get; set; } = string.Empty;
    public string? TeacherAvatarUrl { get; set; }

    //public ICollection<Student> Students { get; set; } = new List<Student>();
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    public string? SuspensionReason { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<AcademicYearTeacher> AcademicYears { get; set; } = [];
    public ICollection<Assistant> Assistants { get; set; } = [];
    public TeacherLandingSettings? TeacherLandingSettings { get; set; }
    public TeacherPreferences Preferences { get; set; } = null!;

    public VideoLibrary? VideoLibrary { get; private set; }
    public int? VideoLibraryId { get; private set; }

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
        string thirdName,
        string lastName
    )
    {
        // 1. Instantiating / converting to Value Objects
        var fullNameResult = ValueObjects.FullName.Create(
            firstName,
            secondName,
            thirdName,
            lastName
        );
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

    public void AttachVideoLibrary(VideoLibrary library)
    {
        ArgumentNullException.ThrowIfNull(library);
        VideoLibraryId = library.Id;
        VideoLibrary = library;
    }
}
