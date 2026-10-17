using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
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

        var quiz = await unitOfWork.GetOrCreateRepository<Quiz, int>().
            FirstOrDefaultAsync(new QuizWithQuestionsSpecification(attempt.QuizId), ct);

        if (quiz is null)
            return Result<SubmitQuizResultDto>.Error("الاختبار غير موجود");

        // Idempotent: a retried submit (lost response) or a lazy auto-submit is not an error
        if (attempt.Status != QuizAttemptStatus.InProgress)
            return Result<SubmitQuizResultDto>.Success(ToDto(attempt, quiz, wasLate: false));

        // Same cutoff as SaveQuizAnswer, so both endpoints agree on "late"
        var acceptAnswersUntil = attempt.StartedAt + quiz.TimeInMinutes
                                 + TimeSpan.FromSeconds(QuizConstants.GracePeriodSeconds);
        var isLate = DateTimeOffset.UtcNow > acceptAnswersUntil;

        // A late submit never changes answers: only what was already saved gets graded
        if (!isLate && request.Answers is { Count: > 0 })
            await ApplyAnswersAsync(attempt, quiz, studentId, request.Answers, ct);


        try
        {
            // Answers + status + score must be committed by the same SaveChanges
            await QuizFinalizer.FinalizeAttempt(attempt, quiz, unitOfWork, ct);
        }
        catch (UniqueConstraintViolationException)
        {
            // A concurrent autosave inserted the same row first. Retryable: the retry finds it.
            return Result<SubmitQuizResultDto>.Conflict("حدث تعارض أثناء التسليم، حاولي تاني");
        }

        return Result<SubmitQuizResultDto>.Success(ToDto(attempt, quiz, isLate));

    }

    private async Task ApplyAnswersAsync(QuizAttempt attempt, Quiz quiz, Guid studentId,
       List<SubmitAnswerDto> answers, CancellationToken ct)
    {
        // Questions linked to this quiz (Quiz.Questions are QuestionLessonQuiz links)
        var validQuestionIds = quiz.Questions.Select(l => l.QuestionId).ToHashSet();

        // One query validates every submitted choice (id + owning question)
        var choiceIds = answers.Where(a => a.ChoiceId is not null)
            .Select(a => a.ChoiceId!.Value).Distinct().ToList();

        var validChoices = new HashSet<(int QuestionId, int ChoiceId)>();
        if (choiceIds.Count > 0)
        {
            var choices = await unitOfWork.GetOrCreateRepository<Choice, int>()
                .ListAsync(new ChoicesByIdsSpecification(choiceIds), ct);
            validChoices = choices.Select(c => (c.QuestionId, c.Id)).ToHashSet();
        }

        var existingByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);

        foreach (var dto in answers.GroupBy(a => a.QuestionId).Select(g => g.MaxBy(a => a.ClientSeq)!))
        {
            // Skip invalid items instead of failing: a submit must never lose the other answers
            if (!validQuestionIds.Contains(dto.QuestionId)) continue;
            if (dto.ChoiceId is not null && !validChoices.Contains((dto.QuestionId, dto.ChoiceId.Value))) continue;

            var text = string.IsNullOrWhiteSpace(dto.TextAnswer) ? null : dto.TextAnswer;

            if (existingByQuestion.TryGetValue(dto.QuestionId, out var row))
            {
                if (dto.ClientSeq <= row.ClientSeq) continue; // newer data already stored
                row.ChoiceId = dto.ChoiceId;
                row.TextAnswer = text;
                row.ClientSeq = dto.ClientSeq;
            }
            else if (dto.ChoiceId is not null || text is not null)
            {
                attempt.Answers.Add(new AttemptAnswer
                {
                    QuizAttemptId = attempt.Id,
                    StudentId = studentId,
                    QuestionId = dto.QuestionId,
                    ChoiceId = dto.ChoiceId,
                    TextAnswer = text,
                    ClientSeq = dto.ClientSeq
                });
            }
        }
    }

    private static SubmitQuizResultDto ToDto(QuizAttempt attempt, Quiz quiz, bool wasLate) => new()
    {
        Status = attempt.Status == QuizAttemptStatus.Graded ? "graded" : "submitted",
        Score = attempt.Status == QuizAttemptStatus.Graded ? attempt.Degree : null,
        TotalDegree = quiz.TotalDegree,
        WasLate = wasLate
    };
}
