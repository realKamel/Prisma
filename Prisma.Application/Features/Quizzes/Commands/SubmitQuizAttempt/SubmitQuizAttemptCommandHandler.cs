using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
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
            .FirstOrDefaultAsync(new AttemptByIdAndStudentSpecification(request.AttemptId, studentId), ct);


        if (attempt is null)
            return Result<SubmitQuizResultDto>.Failure("المحاولة غير موجودة");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            return Result<SubmitQuizResultDto>.Failure("تم تسليم هذا الاختبار من قبل");

        var quiz = await unitOfWork.GetOrCreateRepository<Quiz, int>().
            FirstOrDefaultAsync(new QuizWithQuestionsSpecification(attempt.QuizId), ct);
        

        await QuizFinalizer.FinalizeAttempt(attempt, quiz, unitOfWork, ct);

        return Result<SubmitQuizResultDto>.Success(new SubmitQuizResultDto
        {
            Status = attempt.Status == QuizAttemptStatus.Graded ? "graded" : "submitted",
            Score = attempt.Status == QuizAttemptStatus.Graded ? attempt.Degree : null,
            TotalDegree = quiz.TotalDegree
        });

    }
}
