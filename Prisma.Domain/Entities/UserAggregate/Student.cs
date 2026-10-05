using Ardalis.Result;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.ValueObjects.UserDomain;

namespace Prisma.Domain.Entities.UserAggregate;

public class Student : User
{
    // Private parameterless constructor for EF Core / Identity hydration
    // private Student() : base() { }

    public int StreakDays { get; private set; }
    public string ParentPhoneNumber { get; set; }

    public int? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
    public ICollection<SectionProgress> SectionProgresses { get; private set; } = new List<SectionProgress>();

    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; private set; } =
        new List<AssignmentSubmission>();

    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();
    public ICollection<AttemptAnswer> AttemptAnswers { get; private set; } = new List<AttemptAnswer>();
    public ICollection<QuizAttempt> QuizAttempts { get; private set; } = new List<QuizAttempt>();
    public ICollection<Report> Reports { get; private set; } = new List<Report>();
    public ICollection<TeacherStudent> TeacherStudents { get; private set; } = new List<TeacherStudent>();

    /// <summary>
    /// Factory method to validate value objects and construct a Student instance.
    /// </summary>
    public static Result<Student> Create(
        string firstName,
        string secondName,
        string rawEmail,
        string rawPhone,
        int? academicYearId = null,
        string? rawParentPhone = null,
        string? thirdName = null,
        string? lastName = null)
    {
        // 1. Validate Base Domain Value Objects
        var fullNameResult = ValueObjects.FullName.Create(firstName, secondName, thirdName, lastName);
        var emailAddressResult = EmailAddress.Create(rawEmail);
        var mobilePhoneNumberResult = ValueObjects.UserDomain.PhoneNumber.Create(rawPhone);

        // Optional Parent Phone Validation
        Result<PhoneNumber>? parentPhoneResult = null;
        if (!string.IsNullOrWhiteSpace(rawParentPhone))
        {
            parentPhoneResult = ValueObjects.UserDomain.PhoneNumber.Create(rawParentPhone);
        }

        // 2. Aggregate Validation Errors
        var validationErrors = new List<ValidationError>();

        if (!fullNameResult.IsSuccess)
            validationErrors.AddRange(fullNameResult.ValidationErrors);

        if (!emailAddressResult.IsSuccess)
            validationErrors.AddRange(emailAddressResult.ValidationErrors);

        if (!mobilePhoneNumberResult.IsSuccess)
            validationErrors.AddRange(mobilePhoneNumberResult.ValidationErrors);

        if (parentPhoneResult is { IsSuccess: false })
            validationErrors.AddRange(parentPhoneResult.ValidationErrors);

        if (validationErrors.Count > 0)
            return Result.Invalid(validationErrors);

        // 3. Construct Student Aggregate
        var student = new Student
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            SecondName = secondName,
            ThirdName = thirdName,
            LastName = lastName,
            FullName = fullNameResult.Value,
            EmailAddress = emailAddressResult.Value,
            MobilePhoneNumber = mobilePhoneNumberResult.Value,

            // Sync Base IdentityUser fields
            UserName = emailAddressResult.Value.Value,
            Email = emailAddressResult.Value.Value,
            PhoneNumber = mobilePhoneNumberResult.Value.Value,

            // Student Specific Properties
            AcademicYearId = academicYearId,
            ParentPhoneNumber = parentPhoneResult?.Value,
            StreakDays = 0,
            IsBlocked = false,
            IsOnline = false,
        };

        return Result.Success(student);
    }


    public void UpdateAcademicYear(int academicYearId)
    {
        AcademicYearId = academicYearId;
    }

    public void IncrementStreak()
    {
        StreakDays++;
    }

    public void ResetStreak()
    {
        StreakDays = 0;
    }
}