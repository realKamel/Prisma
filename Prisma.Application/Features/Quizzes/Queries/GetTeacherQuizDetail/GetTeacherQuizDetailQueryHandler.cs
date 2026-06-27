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

namespace Prisma.Application.Features.Quizzes.Queries.GetTeacherQuizDetail;

public class GetTeacherQuizDetailQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherQuizDetailQuery, Result<TeacherQuizDetailDto>>

{
    public async Task<Result<TeacherQuizDetailDto>> Handle(GetTeacherQuizDetailQuery request, CancellationToken ct)
    {
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        var quiz = await quizRepo.FirstOrDefaultAsync(
            new TeacherQuizDetailSpecification(request.QuizId), ct);

        if (quiz is null)
            return Result<TeacherQuizDetailDto>.Failure("الاختبار غير موجود");


        // Compute attempts stats
        var gradedAttempts = quiz.Attempts
            .Where(a => a.Status == QuizAttemptStatus.Graded)
            .ToList();

        var pendingGradingCount = quiz.Attempts
            .Count(a => a.Status == QuizAttemptStatus.Submitted);

        var submittedCount = quiz.Attempts
            .Count(a => a.Status == QuizAttemptStatus.Submitted
                     || a.Status == QuizAttemptStatus.Graded);

        double? averageScore = gradedAttempts.Count > 0 && quiz.TotalDegree > 0
            ? Math.Round(
                gradedAttempts.Average(a => (double)(a.Degree / quiz.TotalDegree * 100)), 1)
            : null;

        string status;
        if (pendingGradingCount > 0)
            status = "pending_grading";
        else if (quiz.Attempts.Any() && quiz.Attempts.All(a => a.Status == QuizAttemptStatus.Graded))
            status = "completed";
        else
            status = "active";

        var questions = quiz.Questions.Select(ql =>
        {
            var q = ql.Question;

            List<TeacherQuizChoiceDto>? choices = null;
            if (q is MCQQuestion mcq)
            {
                choices = mcq.Choices.Select(c => new TeacherQuizChoiceDto
                {
                    ChoiceId = c.Id,
                    Text = c.Text ?? string.Empty,
                    IsCorrect = c.IsCorrect
                }).ToList();
            }

            return new TeacherQuizQuestionDto
            {
                QuestionId = q.Id,
                Text = q.Title,
                Type = q.Type,
                Degree = ql.Degree,
                Choices = choices,
                ModelAnswer = q is WrittenQuestion w ? w.Answer : null
            };
        }).ToList();

        return new TeacherQuizDetailDto
        {
            QuizId = quiz.Id,
            Title = quiz.Title ?? string.Empty,
            Description = quiz.Description,
            Scope = quiz.Scope.ToString(),
            LessonId = quiz.LessonId,
            LessonTitle = quiz.Lesson?.Title,
            AcademicYearId = quiz.AcademicYearId,
            AcademicYearName = quiz.AcademicYear?.Title,
            DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
            TotalDegree = quiz.TotalDegree,
            AvailableFrom = quiz.AvailableFrom,
            DueDate = quiz.DueDate,
            SubmittedCount = submittedCount,
            PendingGradingCount = pendingGradingCount,
            AverageScore = averageScore,
            Status = status,
            Questions = questions

        };

    }
}
