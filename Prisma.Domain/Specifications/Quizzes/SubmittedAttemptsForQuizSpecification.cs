using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Specifications.Quizzes;

public class SubmittedAttemptsForQuizSpecification : Specification<QuizAttempt>
{
    public SubmittedAttemptsForQuizSpecification(int quizId)
    {
        Query.Where(a =>
            a.QuizId == quizId &&
            a.Status != QuizAttemptStatus.InProgress);
    }
}