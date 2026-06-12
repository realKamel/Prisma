using Prisma.Domain.Common;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class Lesson : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsEligible { get; set; }

    // public Guid TeacherId { get; set; }
    // public Teacher Teacher { get; set; }

    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<LessonQuiz> Quizzes { get; set; } = new List<LessonQuiz>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<RedeemCode> RedeemCodes { get; set; } = new List<RedeemCode>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}