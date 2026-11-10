using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Queries.GetQuizStructure;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.SaveQuizAnswer;

public class SaveQuizAnswerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ISender sender)
    : IRequestHandler<SaveQuizAnswerCommand, Result>
{
    public async Task<Result> Handle(SaveQuizAnswerCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();

        // The only read from the DB: per-student data, never cached
        var attempt = await attemptRepo
            .FirstOrDefaultAsync(new AttemptWithAnswerForQuestionSpecification(request.AttemptId, studentId, request.QuestionId), ct);

        if (attempt is null)
            return Result.Error("المحاولة غير موجودة");

        if (attempt.Status != QuizAttemptStatus.InProgress)
            return Result.Error("لا يمكن تعديل الإجابات بعد التسليم");

        
        var acceptUntil = attempt.Deadline + TimeSpan.FromSeconds(QuizConstants.GracePeriodSeconds);
        if (DateTimeOffset.UtcNow > acceptUntil)
            return Result.Error("انتهى وقت الاختبار، لا يمكن حفظ المزيد من الإجابات");

        // Whitespace-only text means "cleared"
        var text = string.IsNullOrWhiteSpace(request.TextAnswer) ? null : request.TextAnswer;


        // Identical for every student: served from the cache after the first request per quiz.
        // Read-only here: the cached object is shared, never modify it.
        var structure = await sender.Send(new GetQuizStructureQuery(attempt.QuizId), ct);


        var question = structure.Questions.FirstOrDefault(q => q.QuestionId == request.QuestionId);

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

            if (request.ChoiceId is not null &&
                question.Choices?.Any(c => c.ChoiceId == request.ChoiceId.Value) != true)
                return Result.Error("الاختيار غير موجود في هذا السؤال");
        }


        var existing =  attempt.Answers.FirstOrDefault();

        if (existing is null)
        {
           unitOfWork.GetOrCreateRepository<AttemptAnswer, int>().Add(new AttemptAnswer
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
            return Result.Conflict("حدث تعارض أثناء الحفظ، سيتم إعادة المحاولة");
        }
        catch (ConcurrencyConflictException)
        {
            // Another save (or Submit/finalize) updated this answer first.
            // Retryable: the retry re-reads and is either applied or ignored as stale by ClientSeq.
            return Result.Conflict("حدث تعارض أثناء الحفظ، سيتم إعادة المحاولة");
        }
        return Result.SuccessWithMessage("تم حفظ الإجابة");

    }
}
