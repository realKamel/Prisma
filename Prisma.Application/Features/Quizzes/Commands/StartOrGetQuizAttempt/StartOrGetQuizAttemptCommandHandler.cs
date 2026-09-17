using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.StartOrGetQuizAttempt;

public class StartOrGetQuizAttemptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<StartOrGetQuizAttemptCommand, Result<AttemptStateDto>>
{
    public async Task<Result<AttemptStateDto>> Handle(StartOrGetQuizAttemptCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;

        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        var timing = await quizRepo.FirstOrDefaultAsync(new QuizTimingSpecification(request.QuizId), ct);

        if (timing is null)
            return Result<AttemptStateDto>.Error("الاختبار غير موجود");

        var now = DateTimeOffset.UtcNow;

        if (timing.AvailableFrom.HasValue && timing.AvailableFrom > now)
            return Result<AttemptStateDto>.Error("الاختبار غير متاح حاليًا");

        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var attempt = await attemptRepo.FirstOrDefaultAsync(
            new StudentAttemptWithAnswersSpecification(timing.Id, studentId), ct);

        if (attempt is not null && attempt.Status == QuizAttemptStatus.InProgress)
        {
            var deadline = attempt.StartedAt + timing.TimeInMinutes;
            var hardDeadline = deadline + TimeSpan.FromSeconds(10);
            if (now >= hardDeadline)
            {

                // Full quiz (with correct answers) only fetched here — the rare timeout path
                var quizForFinalization = await quizRepo.FirstOrDefaultAsync(
                    new QuizForFinalizationSpecification(timing.Id), ct);

                await QuizFinalizer.FinalizeAttempt(attempt, quizForFinalization!, unitOfWork, ct);
                return Result<AttemptStateDto>.Error("انتهى وقت هذه المحاولة");
            }
        }

        if (attempt is not null && attempt.Status != QuizAttemptStatus.InProgress)
            return Result<AttemptStateDto>.Error("سبق أن قمت بتسليم هذا الاختبار");

        if (attempt is null)
        {
            if (timing.DueDate.HasValue && timing.DueDate < now)
                return Result<AttemptStateDto>.Error("انتهى موعد هذا الاختبار");

            attempt = new QuizAttempt
            {
                QuizId = timing.Id,
                StudentId = studentId,
                StartedAt = now,
                Status = QuizAttemptStatus.InProgress
            };
            attemptRepo.Add(attempt);
            await unitOfWork.SaveChangesAsync(ct);
        }

        var dto = new AttemptStateDto
        {
            AttemptId = attempt.Id,
            RemainingSeconds = Math.Max(0, (int)((attempt.StartedAt + timing.TimeInMinutes - now).TotalSeconds)),
            SavedAnswers = attempt.Answers.ToDictionary(
                a => a.QuestionId,
                a => new SavedAnswerDto { SelectedChoiceId = a.ChoiceId, TextAnswer = a.TextAnswer })
        };

        return Result<AttemptStateDto>.Success(dto);
    }
}