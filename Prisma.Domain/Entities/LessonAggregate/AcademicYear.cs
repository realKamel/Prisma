using Prisma.Domain.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class AcademicYear : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<AcademicYearTeacher> Teachers { get; set; } = new List<AcademicYearTeacher>();
    public ICollection<AcademicYearLesson> Lessons { get; set; } = new List<AcademicYearLesson>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}