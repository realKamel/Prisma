using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class AttemptAnswerByAttemptAndQuestionSpecification : Specification<AttemptAnswer>
{
    public AttemptAnswerByAttemptAndQuestionSpecification(int attemptId, int questionId)
    {
        Query
            .Where(a => a.QuizAttemptId == attemptId && a.QuestionId == questionId);
    }
}
