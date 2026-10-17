
using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

// The question must be linked to this quiz (via QuestionLessonQuiz)
public sealed class QuestionInQuizSpecification : Specification<Question>
{
    public QuestionInQuizSpecification(int quizId, int questionId)
    {
        Query.Where(q => q.Id == questionId
                        && q.QuestionLessons.Any(l => l.LessonQuizId == quizId));
    }
}