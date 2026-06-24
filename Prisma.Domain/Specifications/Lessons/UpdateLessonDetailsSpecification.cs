using System;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Teachers;

public class UpdateLessonDetailsSpecification : Specification<Lesson>
{
    public UpdateLessonDetailsSpecification(int lessonId)
        : base()
    {
        // 1. شرط جلب الدرس والتأكد إنه مش ممسوح سوفت ديليت
        Query.Where(lesson => lesson.Id == lessonId && !lesson.IsDeleted)
             .Include(l => l.Sections)
             .Include(l => l.Assignment)
             .Include(l => l.AcademicYears);

    }
}