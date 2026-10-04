using Ardalis.Result;

namespace Prisma.Domain.ValueObjects;

public sealed record FullName
{
    public string FirstName { get; init; } = default!;
    public string SecondName { get; set; }
    public string? ThirdName { get; set; }
    public string? LastName { get; set; }

    // Computed property for display
    public string DisplayName => $"{FirstName} {SecondName}";

    // Private constructor required by EF Core
    private FullName() { }

    private FullName(string firstName, string secondName, string? thirdName, string? lastName)
    {
        FirstName = firstName;
        SecondName = secondName;
        ThirdName = thirdName;
        LastName = lastName;
    }

    /// <summary>
    /// Factory method returning Result
    /// </summary>
    public static Result<FullName> Create(string firstName, string secondName, string? thirdName = null,
        string? lastName = null)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(FirstName), ErrorMessage = "First name is required."
            });
        }
        else if (firstName.Length > 50)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(FirstName), ErrorMessage = "First name cannot exceed 50 characters."
            });
        }

        if (string.IsNullOrWhiteSpace(secondName))
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(SecondName), ErrorMessage = "Second name is required."
            });
        }
        else if (secondName.Length > 50)
        {
            errors.Add(new ValidationError
            {
                Identifier = nameof(SecondName), ErrorMessage = "Second name cannot exceed 50 characters."
            });
        }

        if (errors.Count != 0)
        {
            return Result.Invalid(errors);
        }

        return Result.Success(new FullName(
            firstName.Trim(),
            secondName.Trim(),
            string.IsNullOrWhiteSpace(thirdName) ? null : thirdName.Trim(),
            string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim()
        ));
    }

    public override string ToString() => DisplayName;

    public static implicit operator string(FullName name)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        return name.DisplayName;
    }
}