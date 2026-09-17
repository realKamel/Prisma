using Ardalis.Specification;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Specifications.Quizzes;

public class QuizTimingSpecification : Specification<Quiz, QuizTimingInfo>
{
    public QuizTimingSpecification(int quizId)
    {
        Query
            .Where(q => q.Id == quizId)
            .Select(q => new QuizTimingInfo(q.Id, q.TimeInMinutes, q.AvailableFrom, q.DueDate));
    }
}

public record QuizTimingInfo(int Id, TimeSpan TimeInMinutes, DateTimeOffset? AvailableFrom, DateTimeOffset? DueDate);