namespace Prisma.Application.Features.Quizzes.Dtos;

public class AttemptStateDto
{
    public int AttemptId { get; init; }
    public int RemainingSeconds { get; init; }
    public Dictionary<int, SavedAnswerDto> SavedAnswers { get; init; } = [];
}

public class SavedAnswerDto
{
    public int? SelectedChoiceId { get; init; }
    public string? TextAnswer { get; init; }
}