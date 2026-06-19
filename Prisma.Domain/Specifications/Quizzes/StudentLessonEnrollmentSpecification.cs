using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.EnrollmentAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class StudentLessonEnrollmentSpecification: Specification<Enrollment>
{
    public StudentLessonEnrollmentSpecification(Guid studentId, int lessonId)
    {
        Query.Where(e => e.StudentId == studentId && e.LessonId == lessonId);
    }
}
