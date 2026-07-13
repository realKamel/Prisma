namespace Prisma.Application.Common.DTOs.Ai;

public record AttemptAnswerDto(
    int QuizAttemptId,
    string Question,
    string ModelAnswer,
    string? StudentAnswer,
    decimal MaxScore);