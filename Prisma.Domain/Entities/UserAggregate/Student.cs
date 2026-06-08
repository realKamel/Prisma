using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Entities.UserAggregate;

public class Student : User
{
    public string SecondName { get; set; }
    public string ThirdName { get; set; }

    public int StreakDays { get; set; }

    public string? ParentPhoneNumber { get; set; }

    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public int? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<SectionProgress> SectionProgresses { get; set; } = new List<SectionProgress>();

    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } =
        new List<AssignmentSubmission>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}