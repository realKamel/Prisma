using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Common;
using Prisma.Domain.ValueObjects;
using Prisma.Domain.ValueObjects.UserDomain;
using System.Collections.Frozen;

namespace Prisma.Domain.Entities.UserAggregate;

public class User : IdentityUser<Guid>, IEntity<Guid>, IAuditable, IDomainEventEnumerable
{
    // private User() : base() { } // EF Core private constructor
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string? ThirdName { get; set; }
    public string? LastName { get; set; }

    public FullName? FullName { get; set; }
    public PhoneNumber? MobilePhoneNumber { get; set; }
    public EmailAddress? EmailAddress { get; set; }

    public bool IsBlocked { get; set; }
    public bool IsOnline { get; set; }
    public string? PasswordResetCode { get; set; }
    public bool PasswordResetConfirmed { get; set; }
    public DateTimeOffset? PasswordResetCodeExpiry { get; set; }
    public int ResetPasswordCodeAttemptCount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<IdentityUserClaim<Guid>> Claims { get; } = [];

    public ICollection<UserRole> Roles { get; set; } = [];

    public void MarkAsOnline() => IsOnline = true;

    public void SetOnlineStatus(bool status) => IsOnline = status;

    /// <summary>
    /// Factory method to construct a new User aggregate root while mapping domain value objects
    /// and keeping ASP.NET Core Identity properties in sync.
    /// </summary>
    public static Result<User> Create(
        string firstName,
        string secondName,
        string rawEmail,
        string rawPhone,
        string? thirdName = null,
        string? lastName = null
    )
    {
        // 1. Instantiating / converting to Value Objects
        var fullNameResult = FullName.Create(firstName, secondName, thirdName, lastName);
        var emailAddressResult = EmailAddress.Create(rawEmail);
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
        var user = new User
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

        return Result.Success(user);
    }

    //event dispatching
    private readonly List<BaseDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Keys of the onboarding tours this user has finished or skipped.</summary>
    public ICollection<string> SeenTours { get; private set; } = new List<string>();

    /// <summary>The tours this kind of user can take. Each user type overrides it.</summary>
    protected virtual IReadOnlySet<string> AllowedTours => FrozenSet<string>.Empty;

    /// <summary>
    /// Records that a tour was finished or skipped. Safe to call repeatedly.
    /// Returns true when the key was new, so the caller can skip saving otherwise.
    /// </summary>
    public Result<bool> MarkTourSeen(string? tourKey)
    {
        var key = tourKey?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(key) || !AllowedTours.Contains(key))
        {
            return Result<bool>.Invalid(new ValidationError(nameof(tourKey), "Unknown tour."));
        }

        if (SeenTours.Contains(key))
            return Result.Success(false);

        SeenTours.Add(key);
        return Result.Success(true);
    }
    protected void AddDomainEvent(BaseDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public new Guid Id
    {
        get => base.Id;
        init => base.Id = value;
    }
}
