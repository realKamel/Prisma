using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class TeacherQuizDetailSpecification : Specification<Quiz>
{
    public TeacherQuizDetailSpecification(int quizId)
    {
        Query
            .Where(q => q.Id == quizId)
            .Include(q => q.Questions)
                .ThenInclude(ql => ql.Question)
                    .ThenInclude(q => (q as MCQQuestion)!.Choices)
            .Include(q => q.Attempts)
            .Include(q => q.Lesson)
            .Include(q => q.AcademicYear);
    }
}
