using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
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
            return Result.Error("المحاولة غير موجودة");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            return Result.Error("لا يمكن تعديل الإجابات بعد التسليم");

        var quiz = await unitOfWork.GetOrCreateRepository<Quiz, int>()
        .FirstOrDefaultAsync(new QuizByIdSpecification(attempt.QuizId), ct);

        var deadline = attempt.StartedAt + quiz!.TimeInMinutes + TimeSpan.FromSeconds(QuizConstants.GracePeriodSeconds);
        if (DateTimeOffset.UtcNow > deadline)
            return Result.Error("انتهى وقت الاختبار، لا يمكن حفظ المزيد من الإجابات");

        // Whitespace-only text means "cleared"
        var text = string.IsNullOrWhiteSpace(request.TextAnswer) ? null : request.TextAnswer;

        var question = await unitOfWork.GetOrCreateRepository<Question, int>()
            .FirstOrDefaultAsync(new QuestionInQuizSpecification(attempt.QuizId, request.QuestionId), ct);

        if (question is null)
            return Result.Error("السؤال غير موجود في هذا الاختبار");

        if (question.Type == QuestionType.Written)
        {
            if (request.ChoiceId is not null)
                return Result.Error("هذا السؤال مقالي ولا يقبل اختياراً");
        }

        else
        {
            if (text is not null)
                return Result.Error("هذا السؤال لا يقبل إجابة نصية");

            if (request.ChoiceId is not null)
            {
                var choice = await unitOfWork.GetOrCreateRepository<Choice, int>()
                    .FirstOrDefaultAsync(new ChoiceForQuestionSpecification(request.QuestionId, request.ChoiceId.Value), ct);

                if (choice is null)
                    return Result.Error("الاختيار غير موجود في هذا السؤال");
            }
        }

        var answerRepo = unitOfWork.GetOrCreateRepository<AttemptAnswer, int>();

        var existing = await answerRepo.FirstOrDefaultAsync(
            new AttemptAnswerByAttemptAndQuestionSpecification(attempt.Id, request.QuestionId), ct);

        if (existing is null)
        {
            answerRepo.Add(new AttemptAnswer
            {
                QuizAttemptId = attempt.Id,
                StudentId = studentId,
                QuestionId = request.QuestionId,
                ChoiceId = request.ChoiceId,
                TextAnswer = text,
                ClientSeq = request.ClientSeq
            });
        }
        else
        {
            // Stale write (e.g. a retried request arriving after a newer save): ignore it,
            // but report success so the client does not keep retrying.
            if (request.ClientSeq <= existing.ClientSeq)
                return Result.SuccessWithMessage("تم حفظ الإجابة");

            existing.ChoiceId = request.ChoiceId;
            existing.TextAnswer = text;
            existing.ClientSeq = request.ClientSeq;
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException) when (existing is null)
        {
            // A concurrent request inserted the same (attempt, question) row first.
            // 409 is retryable on the client; the retry will take the update path.
            return Result.Conflict("حدث تعارض أثناء الحفظ، سيتم إعادة المحاولة");
        }
        return Result.SuccessWithMessage("تم حفظ الإجابة");

    }
}
