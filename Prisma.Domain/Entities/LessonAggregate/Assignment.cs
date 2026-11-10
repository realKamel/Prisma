using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class Assignment : BaseEntity
{
    public string? ContentURL { get; set; }
    public DateTimeOffset? DueDate { get; set; }

    public string? Title { get; set; }

    public int LessonId { get; private set; }

    public Lesson Lesson { get; private set; } = null!;

    public decimal TotalMarks { set; get; }

    public ICollection<AssignmentSubmission> Submissions { get; set; } = [];

    public TimeDuration Expiry { get; private set; } = TimeDuration.Zero;

    public Guid FileObjectId { get; private set; }

    public Assignment() { } // ef core

    internal static Result<Assignment> Create(
        Guid fileObjectId,
        decimal totalMark,
        DateTimeOffset? dueDate,
        string? title = null
    )
    {
        if (fileObjectId == Guid.Empty)
        {
            return Result.Invalid(
                new ValidationError(nameof(FileObjectId), "File object ID is required.")
            );
        }

        if (totalMark <= 0)
        {
            return Result.Invalid(
                new ValidationError(nameof(TotalMarks), "Total mark must be greater than zero.")
            );
        }

        return Result.Success(
            new Assignment
            {
                FileObjectId = fileObjectId,
                TotalMarks = totalMark,
                DueDate = dueDate,
                Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            }
        );
    }

    internal void AttachTo(Lesson lesson)
    {
        Lesson = lesson;
    }
}
