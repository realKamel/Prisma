using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetGradingAttemptDetail;

public class GetGradingAttemptDetailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetGradingAttemptDetailQuery, Result<GradingAttemptDetailDto>>
{
    public async Task<Result<GradingAttemptDetailDto>> Handle(
        GetGradingAttemptDetailQuery request, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var attempt = await attemptRepo.FirstOrDefaultAsync(
            new AttemptDetailForGradingSpecification(request.AttemptId), ct);

        if (attempt is null)
            return Result<GradingAttemptDetailDto>.Failure("المحاولة غير موجودة");

        if (attempt.Status == QuizAttemptStatus.InProgress)
            return Result<GradingAttemptDetailDto>.Failure("الطالب لسه في الاختبار");

        var answersByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);

        var questions = attempt.Quiz.Questions.Select(ql =>
        {
            var q = ql.Question;
            answersByQuestion.TryGetValue(q.Id, out var answer);

            List<GradingChoiceDto>? choices = null;
            if (q is MCQQuestion mcq)
            {
                choices = mcq.Choices.Select(c => new GradingChoiceDto
                {
                    ChoiceId = c.Id,
                    Text = c.Text ?? string.Empty,
                    IsCorrect = c.IsCorrect
                }).ToList();
            }

            return new GradingQuestionDto
            {
                QuestionId = q.Id,
                AnswerId = answer?.Id ?? 0,
                Text = q.Title,
                Type = q.Type,
                Degree = ql.Degree,
                Score = answer?.Score,
                IsCorrect = answer?.IsCorrect,
                Choices = choices,
                SelectedChoiceId = answer?.ChoiceId,
                TextAnswer = answer?.TextAnswer,
                // Model answer only shown for written questions
                ModelAnswer = q is WrittenQuestion w ? w.Answer : null
            };
        }).ToList();

        return new GradingAttemptDetailDto
        {
            AttemptId = attempt.Id,
            StudentName = $"{attempt.Student.FirstName} {attempt.Student.LastName}".Trim(),
            QuizTitle = attempt.Quiz.Title ?? string.Empty,
            SubmittedAt = attempt.SubmittedAt,
            TotalDegree = attempt.Quiz.TotalDegree,
            Score = attempt.Status == QuizAttemptStatus.Graded ? attempt.Degree : null,
            PenaltyScore = attempt.PenaltyScore,
            Status = attempt.Status == QuizAttemptStatus.Submitted ? "submitted" : "graded",
            //HeldForSecurityReview = (attempt.TabSwitchCount + attempt.CopyPasteAttemptCount) > 0,
            //HeldForManualGrading = attempt.Answers.Any(a => a.Score == null),
            Questions = questions
        };
    }
}
