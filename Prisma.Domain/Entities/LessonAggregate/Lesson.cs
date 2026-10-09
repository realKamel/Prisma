using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Lesson : BaseEntity
{
    public Guid PublicId { get; init; } = Guid.CreateVersion7();
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Money Money { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeDuration TimeDuration { get; set; } = TimeDuration.Zero;
    public string? ImageThumbnailUrl { get; set; }

    public string? VideoUrl { get; set; }

    public LessonStatus Status { get; set; }

    public DateTimeOffset? EndDate { get; set; }
    public DateRange? ValidityRange { get; set; }

    public bool IsEligible { get; set; }

    public string? Transcript { get; set; }
    public string? Summary { get; set; }

    public ICollection<LessonTranscriptChunk> Chunks { get; set; } = [];

    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public ICollection<AcademicYearLesson> AcademicYears { get; set; } = [];

    public ICollection<Section> Sections { get; set; } = [];

    // public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public int? AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    // public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public int? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<RedeemCode> RedeemCodes { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<LessonMaterial> LessonMaterials { get; set; } = [];

    public ICollection<string> Outcomes { get; set; } = [];

    //self-relation
    public int? PrerequisiteId { get; set; }
    public Lesson? Prerequisite { get; set; }

    //TODO:complete the impl

    // private Lesson() { }

    /// <summary>
    /// Factory method to create a lesson in valid state
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="prerequisiteId"></param>
    /// <param name="isPublished"></param>
    /// <param name="outcomes"></param>
    /// <param name="teacherId"></param>
    /// <param name="price"></param>
    /// <param name="currency"></param>
    /// <param name="durationInSeconds"></param>
    /// <returns></returns>
    public static Result<Lesson> Create(
        string title,
        string? description,
        int prerequisiteId,
        bool isPublished,
        IReadOnlyList<string> outcomes,
        Guid teacherId,
        decimal price,
        string currency,
        int durationInSeconds
    )
    {
        var moneyResult = Money.Create(price, currency);

        if (!moneyResult.IsSuccess)
        {
            return Result.Invalid(moneyResult.ValidationErrors);
        }

        var lessonStatus = isPublished ? LessonStatus.Active : LessonStatus.Drafted;

        var lesson = new Lesson
        {
            Title = title,
            Description = description,
            Money = moneyResult.Value,
            PrerequisiteId = prerequisiteId,
            TimeDuration = TimeDuration.Create(durationInSeconds),
            Status = lessonStatus,
            Outcomes = [.. outcomes],
            TeacherId = teacherId,
        };

        return Result.Success(lesson);
    }
}
