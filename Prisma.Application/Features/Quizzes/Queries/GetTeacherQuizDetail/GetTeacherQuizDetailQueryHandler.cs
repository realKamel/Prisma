
using Ardalis.Result;
using MediatR;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Application.Features.Quizzes.Specifications;
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
            return Result<TeacherQuizDetailDto>.Error("الاختبار غير موجود");

        var questionLessonRepo = unitOfWork.GetOrCreateRepository<QuestionLessonQuiz, int>();

        var questionLessons = await questionLessonRepo.ListAsync(
            new TeacherQuizQuestionsSpecification(request.QuizId), ct);

        double? averageScore = null;

        if (quiz.AverageDegree.HasValue && quiz.TotalDegree > 0)
        {
            averageScore = Math.Round(
                (double)(quiz.AverageDegree.Value / quiz.TotalDegree * 100),
                1);
        }

        string status;

        if (quiz.PendingGradingCount > 0)
            status = "pending_grading";
        else if (quiz.HasAttempts && !quiz.HasUngradedAttempts)
            status = "completed";
        else
            status = "active";

        var questions = questionLessons.Select(ql =>
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
            QuizId = quiz.QuizId,
            Title = quiz.Title ?? string.Empty,
            Description = quiz.Description,
            Scope = quiz.Scope.ToString(),
            LessonId = quiz.LessonId,
            LessonTitle = quiz.LessonTitle,
            AcademicYearId = quiz.AcademicYearId,
            AcademicYearName = quiz.AcademicYearName,
            DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
            TotalDegree = quiz.TotalDegree,
            AvailableFrom = quiz.AvailableFrom,
            DueDate = quiz.DueDate,
            SubmittedCount = quiz.SubmittedCount,
            PendingGradingCount = quiz.PendingGradingCount,
            AverageScore = averageScore,
            Status = status,
            Questions = questions

        };

    }
}
