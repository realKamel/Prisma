using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptByIdAndStudentSpecification : Specification<QuizAttempt>
{
    public AttemptByIdAndStudentSpecification(int attemptId, Guid studentId)
    {
        Query
            .Where(a => a.Id == attemptId && a.StudentId == studentId)
            .Include(a => a.Answers);
    }
}
