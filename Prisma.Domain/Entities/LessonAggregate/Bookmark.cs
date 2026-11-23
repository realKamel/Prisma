using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Bookmark : BaseEntity
{
    public const int MaxNoteLength = 120;
    public const int MaxPerSection = 20;

    public Guid PublicId { get; init; } = Guid.CreateVersion7();

    /// <summary>Whole seconds into the section's video.</summary>
    public int PositionSeconds { get; private set; }

    public string? Note { get; private set; }

    public Guid StudentId { get; private set; }
    public Student Student { get; private set; } = null!;

    public int SectionId { get; private set; }
    public Section Section { get; private set; } = null!;

    // EF Core
    private Bookmark() { }

    public static Result<Bookmark> Create(
        Guid studentId,
        int sectionId,
        int positionSeconds,
        string? note
    )
    {
        var errors = new List<ValidationError>();

        if (studentId == Guid.Empty)
            errors.Add(new ValidationError(nameof(studentId), "StudentId is required."));

        if (sectionId <= 0)
            errors.Add(new ValidationError(nameof(sectionId), "A valid SectionId is required."));

        if (positionSeconds < 0)
            errors.Add(
                new ValidationError(nameof(positionSeconds), "Position cannot be negative.")
            );

        var noteResult = NormalizeNote(note);
        if (!noteResult.IsSuccess)
            errors.AddRange(noteResult.ValidationErrors);

        if (errors.Count > 0)
            return Result<Bookmark>.Invalid(errors);

        return Result.Success(
            new Bookmark
            {
                StudentId = studentId,
                SectionId = sectionId,
                PositionSeconds = positionSeconds,
                Note = noteResult.Value,
            }
        );
    }

    public Result UpdateNote(string? note)
    {
        var noteResult = NormalizeNote(note);
        if (!noteResult.IsSuccess)
            return Result.Invalid(noteResult.ValidationErrors);

        Note = noteResult.Value;
        return Result.Success();
    }

    // Trims; blank becomes null; rejects anything over MaxNoteLength.
    private static Result<string?> NormalizeNote(string? note)
    {
        var trimmed = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        if (trimmed is { Length: > MaxNoteLength })
        {
            return Result<string?>.Invalid(
                new ValidationError(
                    nameof(note),
                    $"Note cannot exceed {MaxNoteLength} characters."
                )
            );
        }

        return Result.Success(trimmed);
    }
}