using Prisma.Domain.Common;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Lesson : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public TimeSpan Duration { get; set; }

    public string? ImageThumbnailUrl { get; set; }

    public string? VideoUrl { get; set; }

    public LessonStatus Status { get; set; }

    public DateTimeOffset? EndDate { get; set; }

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
}