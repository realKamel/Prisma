using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

public class SaveQuizAnswerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<SaveQuizAnswerCommand, Result>
{
    public async Task<Result> Handle(SaveQuizAnswerCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        
        var attempt = await attemptRepo
            .FirstOrDefaultAsync(new AttemptByIdAndStudentSpecification(request.AttemptId, studentId), ct);

        if (attempt is null)
            return Result.Failure("المحاولة غير موجودة");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            return Result.Failure("لا يمكن تعديل الإجابات بعد التسليم");

        var quiz = await unitOfWork.GetOrCreateRepository<Quiz, int>()
        .FirstOrDefaultAsync(new QuizByIdSpecification(attempt.QuizId), ct);

        var deadline = attempt.StartedAt + quiz!.TimeInMinutes + TimeSpan.FromSeconds(5);
        if (DateTimeOffset.UtcNow > deadline)
            return Result.Failure("انتهى وقت الاختبار، لا يمكن حفظ المزيد من الإجابات");

        var existing = attempt.Answers.FirstOrDefault(a => a.QuestionId == request.QuestionId);

        if (existing is null)
        {
            var answerRepo = unitOfWork.GetOrCreateRepository<AttemptAnswer, int>();
            answerRepo.Add(new AttemptAnswer
            {
                QuizAttemptId = attempt.Id,
                StudentId = studentId,
                QuestionId = request.QuestionId,
                ChoiceId = request.ChoiceId,
                TextAnswer = request.TextAnswer
            });
        }
        else
        {
            existing.ChoiceId = request.ChoiceId;
            existing.TextAnswer = request.TextAnswer;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success("تم حفظ الإجابة");

    }
}
