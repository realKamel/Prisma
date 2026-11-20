using System.Linq.Expressions;
using Ardalis.Specification;
using Prisma.Application.Common.Time;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Common;

internal sealed record PoolRow(int QuestionId, bool Missed);
internal sealed record PoolQuestion(int QuestionId, bool Missed);
internal sealed record ChoiceRow(int QuestionId, string QuestionText, int ChoiceId, string Text, bool IsCorrect);

internal static class PracticePool
{
    public const int RoundSize = 5;

    /// Distinct questions a student needs before practice is offered.
    public const int MinPool = 5;

    /// Rows the eligibility probe reads. Larger than MinPool because one question can appear in several quizzes.
    public const int ProbeRows = 50;

    /// Every MCQ question the student answered in a closed quiz. Missed = wrong at least once.
    public static async Task<List<PoolQuestion>> LoadAsync(
        IUnitOfWork unitOfWork, Guid studentId, CancellationToken ct)
    {
        var rows = await unitOfWork
            .GetOrCreateRepository<AttemptAnswer, int>()
            .ListAsync(new PracticePoolSpec(studentId, DateTimeOffset.UtcNow), ct);

        return rows
            .GroupBy(r => r.QuestionId)
            .Select(g => new PoolQuestion(g.Key, g.Any(r => r.Missed)))
            .OrderBy(p => p.QuestionId) // stable order, so the seeded shuffle is repeatable
            .ToList();
    }

    /// Same student + same Cairo day = same round, so nothing has to be stored.
    /// Missed questions come first, then the rest, each group shuffled.
    public static List<int> Pick(IReadOnlyList<PoolQuestion> pool, Guid studentId, DateOnly today)
    {
        if (pool.Count < MinPool) return [];

        var rng = new Random(studentId.GetHashCode() ^ today.DayNumber);
        var missed = pool.Where(p => p.Missed).OrderBy(_ => rng.Next());
        var rest = pool.Where(p => !p.Missed).OrderBy(_ => rng.Next());

        return missed.Concat(rest).Select(p => p.QuestionId).Take(RoundSize).ToList();
    }

    /// Yes/no only: reads a handful of ids instead of the student's whole history.
    public static async Task<bool> HasPoolAsync(
        IUnitOfWork unitOfWork, Guid studentId, CancellationToken ct)
    {
        var ids = await unitOfWork
            .GetOrCreateRepository<AttemptAnswer, int>()
            .ListAsync(new PracticeProbeSpec(studentId, DateTimeOffset.UtcNow), ct);

        return ids.Distinct().Count() >= MinPool;
    }
}

/// The one definition of "this answer can feed practice". Change it here and all three specs follow.
internal static class PracticeEligibility
{
    public static Expression<Func<AttemptAnswer, bool>> For(Guid studentId, DateTimeOffset now) =>
        a => a.StudentId == studentId
            && a.ChoiceId != null // MCQ only: true/false answers use a flag, written ones have no choice
            && a.QuizAttempt.Status != QuizAttemptStatus.InProgress
            && (a.QuizAttempt.Quiz.DueDate == null || a.QuizAttempt.Quiz.DueDate < now);
}

internal sealed class PracticePoolSpec : Specification<AttemptAnswer, PoolRow>
{
    public PracticePoolSpec(Guid studentId, DateTimeOffset now)
    {
        Query
            .Where(PracticeEligibility.For(studentId, now))
            .Select(a => new PoolRow(a.QuestionId, a.IsCorrect == false));
    }
}

internal sealed class PracticeProbeSpec : Specification<AttemptAnswer, int>
{
    public PracticeProbeSpec(Guid studentId, DateTimeOffset now)
    {
        Query
            .Where(PracticeEligibility.For(studentId, now))
            .OrderBy(a => a.QuestionId)
            .Take(PracticePool.ProbeRows)
            .Select(a => a.QuestionId);
    }
}

/// One EXISTS query: "is this question one of this student's practice questions?"
internal sealed class PracticeAccessSpec : Specification<AttemptAnswer>
{
    public PracticeAccessSpec(Guid studentId, int questionId, DateTimeOffset now)
    {
        Query
            .Where(PracticeEligibility.For(studentId, now))
            .Where(a => a.QuestionId == questionId);
    }
}

internal sealed class PracticeChoicesSpec : Specification<Choice, ChoiceRow>
{
    public PracticeChoicesSpec(List<int> questionIds)
    {
        Query
            .Where(c => questionIds.Contains(c.QuestionId))
            .OrderBy(c => c.Id)
            .Select(c => new ChoiceRow(c.QuestionId, c.Question.Title, c.Id, c.Text, c.IsCorrect));
    }
}