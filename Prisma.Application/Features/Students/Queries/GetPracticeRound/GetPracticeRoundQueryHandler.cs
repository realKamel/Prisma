using Ardalis.Result;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Time;
using Prisma.Application.Features.Students.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Queries.GetPracticeRound;

public sealed class GetPracticeRoundQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IQueryHandler<GetPracticeRoundQuery, Result<PracticeRoundDto>>
{
    public async Task<Result<PracticeRoundDto>> Handle(GetPracticeRoundQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return Result<PracticeRoundDto>.Unauthorized();

        var student = await unitOfWork
            .GetOrCreateRepository<Student, Guid>()
            .GetByIdAsync(userId.Value, ct);
        if (student is null)
            return Result<PracticeRoundDto>.NotFound();

        var today = CairoDate.Today();
        if (student.LastPracticedDate == today)
            return Result.Success(new PracticeRoundDto([], PracticedToday: true));

        var pool = await PracticePool.LoadAsync(unitOfWork, userId.Value, ct);
        var ids = PracticePool.Pick(pool, userId.Value, today);
        if (ids.Count == 0)
            return Result.Success(new PracticeRoundDto([]));

        var rows = await unitOfWork
            .GetOrCreateRepository<Choice, int>()
            .ListAsync(new PracticeChoicesSpec(ids), ct);

        var byQuestion = rows.GroupBy(r => r.QuestionId).ToDictionary(g => g.Key);

        var questions = ids
            .Where(id => byQuestion.TryGetValue(id, out var g) && g.Count() >= 2 && g.Any(c => c.IsCorrect))
            .Select(id =>
            {
                var g = byQuestion[id];
                return new PracticeQuestionDto(
                    id,
                    g.First().QuestionText,
                    g.Select(c => new PracticeChoiceDto(c.ChoiceId, c.Text)).ToList()
                );
            })
            .ToList();

        return Result.Success(new PracticeRoundDto(questions));
    }
}