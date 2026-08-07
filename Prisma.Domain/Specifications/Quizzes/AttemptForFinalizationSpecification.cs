using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptForFinalizationSpecification : Specification<QuizAttempt>
{
    public AttemptForFinalizationSpecification(int attemptId, Guid studentId)
    {
        Query
           .Where(a => a.Id == attemptId && a.StudentId == studentId)
           .Include(a => a.Answers);
    }
}
