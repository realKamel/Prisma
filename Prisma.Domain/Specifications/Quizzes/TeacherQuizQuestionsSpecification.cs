using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class TeacherQuizQuestionsSpecification : Specification<QuestionLessonQuiz>
{
    public TeacherQuizQuestionsSpecification(int quizId)
    {
        Query
            .Where(ql => ql.Id == quizId)
            .Include(ql => ql.Question)
                .ThenInclude(q => (q as MCQQuestion)!.Choices);
    }
}