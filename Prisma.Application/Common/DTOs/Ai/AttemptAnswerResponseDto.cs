namespace Prisma.Application.Common.DTOs.Ai;

public record AttemptAnswerResponseDto(
    int QuizAttemptId,
    decimal Score,
    bool IsCorrect
);