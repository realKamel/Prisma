using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class StudentQuizzesListProjection
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public decimal TotalDegree { get; init; }
    public DateTimeOffset? AvailableFrom { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public int DurationMinutes { get; init; }
    public int QuestionsCount { get; init; }

    public AttemptProjection? Attempt { get; init; }
}

public class AttemptProjection
{
    public int Id { get; init; }
    public QuizAttemptStatus Status { get; init; }
    public decimal? Degree { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
}