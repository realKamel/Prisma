namespace Prisma.Infrastructure.AgenticWorkflows.DTOs;

public record AttemptAnswerDto(
    int QuizAttemptId,
    string Question,
    string ModelAnswer,
    string? StudentAnswer,
    decimal MaxScore);