using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.LessonAggregate;

namespace Prisma.Domain.Specifications.Assignments;

public class AssignmentsWithLessonSpecification : Specification<Assignment>
{
    public AssignmentsWithLessonSpecification()
    {
        Query.Include(a => a.Lesson);
    }
}