using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Lessons;

public class LessonExpiredSpecification : Specification<Lesson>
{
   public LessonExpiredSpecification(int lessonId)
        : base()
    {
        Query.Where(lesson => lesson.Id == lessonId)
             .Include(l => l.Enrollments)
             .Include(l => l.Sections)
             .ThenInclude(s => s.Progresses)
             .Include(l=>l.LessonMaterials)
             .Include(l => l.Quiz)
             .ThenInclude(q => q.Attempts)
             .AsNoTracking();

    }

}
