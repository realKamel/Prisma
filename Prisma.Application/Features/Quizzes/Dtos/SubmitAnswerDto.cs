namespace Prisma.Application.Features.Quizzes.Dtos;

public sealed record SubmitAnswerDto(int QuestionId, int? ChoiceId, string? TextAnswer, long ClientSeq);

