using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.OverrideAttemptScore;

public class OverrideAttemptScoreCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<OverrideAttemptScoreCommand, Result<OverrideScoreResultDto>>
{
    public async Task<Result<OverrideScoreResultDto>> Handle(OverrideAttemptScoreCommand request, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var attempt = await attemptRepo.FirstOrDefaultAsync(
            new AttemptWithQuizSpecification(request.AttemptId), ct);

        if (attempt is null)
            return Result<OverrideScoreResultDto>.Failure("المحاولة غير موجودة");

        // Can only override score for graded or submitted (held for security review) attempts
        if (attempt.Status == QuizAttemptStatus.InProgress)
            return Result<OverrideScoreResultDto>.Failure("الطالب لسه في الاختبار");

        // Penalty can't exceed the current degree
        if (request.PenaltyScore > attempt.Degree)
            return Result<OverrideScoreResultDto>.Failure($"الخصم ({request.PenaltyScore}) أكبر من درجة الطالب الحالية ({attempt.Degree})");

        // Apply penalty
        attempt.PenaltyScore = request.PenaltyScore;
        attempt.Degree -= request.PenaltyScore;
        attempt.Status = QuizAttemptStatus.Graded;

        await unitOfWork.SaveChangesAsync(ct);

        return Result<OverrideScoreResultDto>.Success(
    new OverrideScoreResultDto { FinalScore = attempt.Degree },
    "تم تعديل الدرجة بنجاح"
);
    }
}
