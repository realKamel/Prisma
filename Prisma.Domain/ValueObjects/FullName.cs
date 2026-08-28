namespace Prisma.Domain.ValueObjects;

public record FullName
{
    public string FirstName { get; init; }
    public string SecondName { get; init; }
    public string? ThirdName { get; init; }
    public string? LastName { get; init; }

    // Parameterless constructor for EF Core reflection
    private FullName() { }

    // Main constructor with validation
    public FullName(
        string firstName,
        string secondName,
        string? thirdName = null,
        string? lastName = null
    )
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(secondName))
        {
            throw new ArgumentException("Second name is required.", nameof(secondName));
        }

        FirstName = firstName;
        SecondName = secondName;
        ThirdName = thirdName;
        LastName = lastName;
    }

    public string DisplayName =>
        string.Join(
            " ",
            new[] { FirstName, SecondName, ThirdName, LastName }.Where(x =>
                !string.IsNullOrWhiteSpace(x)
            )
        );
}
