namespace Prisma.Application.Features.Students.Common;

public sealed record PracticeChoiceDto(int Id, string Text);
public sealed record PracticeQuestionDto(int QuestionId, string Text, IReadOnlyList<PracticeChoiceDto> Choices);
public sealed record PracticeRoundDto(IReadOnlyList<PracticeQuestionDto> Questions, bool PracticedToday = false);
public sealed record PracticeAnswerResultDto(bool IsCorrect, int CorrectChoiceId);
public sealed record FinishPracticeResultDto(bool StreakCounted);