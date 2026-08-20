using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Enrollments;

public class EnrollmentWithProjectionSpec<TResult> : Specification<Enrollment, TResult>
{
    public EnrollmentWithProjectionSpec(Guid studentId, Expression<Func<Enrollment, TResult>> projection)
    {
        Query
            .Where(e => e.StudentId == studentId)
            .AsNoTracking()
            .Select(projection);
    }
}