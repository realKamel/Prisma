using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizByIdSpecification : Specification<Quiz>
{
    public QuizByIdSpecification(int quizId)
    {
        Query.Where(q => q.Id == quizId);
    }
}
