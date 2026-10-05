using Ardalis.Result;

namespace Prisma.Domain.Entities.UserAggregate;

public class Assistant : User
{
    // private Assistant() : base() { }
    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    /// <summary>
    /// Factory method to create a validated Assistant instance.
    /// </summary>
    public static Result<Assistant> Create(
        string firstName,
        string secondName,
        string rawEmail,
        string rawPhone,
        Guid? teacherId = null,
        string? thirdName = null,
        string? lastName = null)
    {
        // 1. Convert to / validate Value Objects
        var fullNameResult = ValueObjects.FullName.Create(firstName, secondName, thirdName, lastName);
        var emailAddressResult = ValueObjects.UserDomain.EmailAddress.Create(rawEmail);
        var mobilePhoneNumberResult = ValueObjects.UserDomain.PhoneNumber.Create(rawPhone);

        // 2. Aggregate validation errors
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

        // 3. Initialize Assistant Entity
        var assistant = new Assistant
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            SecondName = secondName,
            ThirdName = thirdName,
            LastName = lastName,
            FullName = fullNameResult.Value,
            EmailAddress = emailAddressResult.Value,
            MobilePhoneNumber = mobilePhoneNumberResult.Value,

            // Sync base IdentityUser properties
            UserName = emailAddressResult.Value.Value,
            Email = emailAddressResult.Value.Value,
            PhoneNumber = mobilePhoneNumberResult.Value.Value,
            TeacherId = teacherId,
            IsBlocked = false,
            IsOnline = false
        };

        return Result.Success(assistant);
    }

    /// <summary>
    /// Domain method to assign or reassign a teacher
    /// </summary>
    public void AssignTeacher(Guid teacherId)
    {
        TeacherId = teacherId;
    }

    /// <summary>
    /// Domain method to remove teacher assignment
    /// </summary>
    public void RemoveTeacher()
    {
        TeacherId = null;
        Teacher = null;
    }
}