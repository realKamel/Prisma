using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.AcademicYearTeacher;

public class AcademicYearTeacherWithProjection<TResult> : Specification<Prisma.Domain.Entities.LessonAggregate.AcademicYearTeacher, TResult>
{
    public AcademicYearTeacherWithProjection(Guid teacherId, Expression<Func<Prisma.Domain.Entities.LessonAggregate.AcademicYearTeacher, TResult>> projection)
    {
        Query
            .Where(a => a.TeacherId == teacherId)
            .AsNoTracking()
            .Select(projection);
    }
}
