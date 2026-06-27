using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class GradingAttemptsSpecification : Specification<QuizAttempt>
{
    public GradingAttemptsSpecification(QuizScope scope, int? quizId)
    {
        Query
            // Only submitted or graded attempts are relevant for grading
            .Where(a => a.Status == QuizAttemptStatus.Submitted
                     || a.Status == QuizAttemptStatus.Graded)
            .Where(a => a.Quiz.Scope == scope)
            .Include(a => a.Student)
            .Include(a => a.Quiz)
            .Include(a => a.Answers);

        if (quizId.HasValue)
            Query.Where(a => a.QuizId == quizId.Value);
    }
}
