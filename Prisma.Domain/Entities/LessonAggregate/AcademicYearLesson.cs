using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class AcademicYearLesson : BaseEntity
{
    public AcademicYear AcademicYear { get; set; }
    public int AcademicYearId { get; set; }

    public Lesson Lesson { get; set; }
    public int LessonId { get; set; }
}
