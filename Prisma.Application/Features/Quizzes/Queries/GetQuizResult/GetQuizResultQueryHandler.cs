using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizResult;

public class GetQuizResultQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<GetQuizResultQuery, Result<QuizResultDto>>
{
    public async Task<Result<QuizResultDto>> Handle(GetQuizResultQuery request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;

        var quiz = await unitOfWork.GetOrCreateRepository<Quiz, int>()
            .FirstOrDefaultAsync(new QuizWithQuestionsSpecification(request.QuizId), ct);

        
        if (quiz is null)
            return Result<QuizResultDto>.Failure("الاختبار غير موجود");

        var attempt = await unitOfWork.GetOrCreateRepository<QuizAttempt, int>().
            FirstOrDefaultAsync(
    new StudentAttemptWithAnswersSpecification(quiz.Id, studentId), ct);

        if (attempt is null || attempt.Status == QuizAttemptStatus.InProgress)
            return Result<QuizResultDto>.Failure("لم يتم تسليم هذا الاختبار بعد");

        var now = DateTimeOffset.UtcNow;

        var dueDatePassed = !quiz.DueDate.HasValue || now >= quiz.DueDate.Value;

        if (!dueDatePassed)
        {
            return Result<QuizResultDto>.Success(new QuizResultDto
            {
                QuizId = quiz.Id,
                Title = quiz.Title ?? string.Empty,
                Status = "locked",
                TotalDegree = quiz.TotalDegree,
                AvailableAt = quiz.DueDate
            });
        }


        if (attempt.Status != QuizAttemptStatus.Graded)
        {
            return Result<QuizResultDto>.Success(new QuizResultDto
            {
                QuizId = quiz.Id,
                Title = quiz.Title ?? string.Empty,
                Status = "pending",
                TotalDegree = quiz.TotalDegree,
                CorrectCount = attempt.Answers.Count(a => a.IsCorrect == true),
                WrongCount = attempt.Answers.Count(a => a.IsCorrect == false),
                PendingCount = attempt.Answers.Count(a => a.Score == null),
                Review = null
            });
        }


        var answersByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);

        var review = quiz.Questions.Select(ql =>
        {
            var q = ql.Question;
            answersByQuestion.TryGetValue(q.Id, out var ans);

            List<QuizReviewChoiceDto>? choices = null;
            if (q is MCQQuestion mcq)
            {
                choices = mcq.Choices.Select(c => new QuizReviewChoiceDto
                {
                    ChoiceId = c.Id,
                    Text = c.Text ?? string.Empty,
                    IsCorrect = c.IsCorrect
                }).ToList();
            }

            return new QuizReviewQuestionDto
            {
                QuestionId = q.Id,
                Text = q.Title,
                Type = q.Type,
                Choices = choices,
                SelectedChoiceId = ans?.ChoiceId,
                TextAnswer = ans?.TextAnswer,
                CorrectWrittenAnswer = q is WrittenQuestion w ? w.Answer : null,
                IsCorrect = ans?.IsCorrect,
                Score = ans?.Score,
                Degree = ql.Degree
            };
        }).ToList();

        return Result<QuizResultDto>.Success(new QuizResultDto
        {
            QuizId = quiz.Id,
            Title = quiz.Title ?? string.Empty,
            Status = "done",
            Score = attempt.Degree,
            TotalDegree = quiz.TotalDegree,
            CorrectCount = attempt.Answers.Count(a => a.IsCorrect == true),
            WrongCount = attempt.Answers.Count(a => a.IsCorrect == false),
            PendingCount = 0,
            GradedAt = attempt.UpdatedAt,
            Review = review
        });
    }
}
