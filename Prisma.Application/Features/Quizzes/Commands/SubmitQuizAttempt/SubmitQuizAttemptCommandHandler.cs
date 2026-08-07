using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.SubmitQuizAttempt;

public class SubmitQuizAttemptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<SubmitQuizAttemptCommand, Result<SubmitQuizResultDto>>
{
    public async Task<Result<SubmitQuizResultDto>> Handle(SubmitQuizAttemptCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;

        var attempt = await unitOfWork.GetOrCreateRepository<QuizAttempt, int>()
            .FirstOrDefaultAsync(new AttemptForFinalizationSpecification(request.AttemptId, studentId), ct);


        if (attempt is null)
            return Result<SubmitQuizResultDto>.Error("المحاولة غير موجودة");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            return Result<SubmitQuizResultDto>.Error("تم تسليم هذا الاختبار من قبل");

        var quiz = await unitOfWork.GetOrCreateRepository<Quiz, int>().
            FirstOrDefaultAsync(new QuizWithQuestionsSpecification(attempt.QuizId), ct);

        if (quiz is null)
            return Result<SubmitQuizResultDto>.Error("الاختبار غير موجود");

        var now = DateTimeOffset.UtcNow;

        var deadline = attempt.StartedAt + quiz!.TimeInMinutes;
        var hardDeadline = deadline + TimeSpan.FromSeconds(10);
        var isLate = now > hardDeadline;

        await QuizFinalizer.FinalizeAttempt(attempt, quiz, unitOfWork, ct);

        if (isLate)
        {
            return Result<SubmitQuizResultDto>.Error("انتهى وقت الاختبار وتم تسليمه تلقائيًا");
        }

        return Result<SubmitQuizResultDto>.Success(new SubmitQuizResultDto
        {
            Status = attempt.Status == QuizAttemptStatus.Graded ? "graded" : "submitted",
            Score = attempt.Status == QuizAttemptStatus.Graded ? attempt.Degree : null,
            TotalDegree = quiz.TotalDegree
        });

    }
}
