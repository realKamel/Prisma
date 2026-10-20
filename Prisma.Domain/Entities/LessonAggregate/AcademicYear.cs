using Prisma.Domain.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public class AcademicYear : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid PublicId { get; set; } = Guid.CreateVersion7();
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<AcademicYearTeacher> Teachers { get; set; } = [];

    public ICollection<AcademicYearLesson> Lessons { get; set; } = [];
    public ICollection<Quiz> Quizzes { get; set; } = [];

    //TODO
    // public ICollection<Subject> Subjects { get; set; } = [];

    public void RegisterTeacherInAcademicYear(Guid teacherId)
    {
        var teacher = new AcademicYearTeacher { TeacherId = teacherId, AcademicYearId = this.Id };
        Teachers.Add(teacher);
    }
}
