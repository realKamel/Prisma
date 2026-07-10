namespace Prisma.Infrastructure.AgenticWorkflows.DTOs;

public record AttemptAnswerResponseDto(
    int QuizAttemptId,
    decimal Score,
    bool IsCorrect
);