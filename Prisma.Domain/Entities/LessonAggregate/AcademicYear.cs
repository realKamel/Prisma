using Ardalis.Result;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Entities.LessonAggregate;

public sealed class AcademicYear : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public Guid PublicId { get; set; } = Guid.CreateVersion7();
    public ICollection<Student> Students { get; set; } = [];
    public ICollection<AcademicYearTeacher> Teachers { get; set; } = [];

    public ICollection<AcademicYearLesson> Lessons { get; set; } = [];
    public ICollection<Quiz> Quizzes { get; set; } = [];

    private AcademicYear() { }

    public AcademicYear(string title)
    {
        Title = title;
        PublicId = Guid.CreateVersion7();
    }

    public Result RegisterTeacher(Guid teacherId)
    {
        if (Teachers.Any(x => x.TeacherId == teacherId))
        {
            return Result.Conflict("Teacher is already registered in this academic year.");
        }

        Teachers.Add(new AcademicYearTeacher(teacherId));

        return Result.Success();
    }
}
