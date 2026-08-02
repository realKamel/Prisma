using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetQuizForTaking;

public class GetQuizForTakingQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<GetQuizForTakingQuery, Result<QuizTakingDto>>
{
    public async Task<Result<QuizTakingDto>> Handle(GetQuizForTakingQuery request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;
        
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();
        var quiz = await quizRepo
            .FirstOrDefaultAsync(new QuizWithQuestionsAndLessonSpecification(request.QuizId), ct);

        if (quiz is null)
            return Result<QuizTakingDto>.Error("الاختبار غير موجود");

        var now = DateTimeOffset.UtcNow;

        if (quiz.AvailableFrom.HasValue && quiz.AvailableFrom > now)
            return Result<QuizTakingDto>.Error("الاختبار غير متاح حاليًا");

        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();

        var attempt = await attemptRepo.FirstOrDefaultAsync(
    new StudentAttemptWithAnswersSpecification(quiz.Id, studentId), ct);

        if (attempt is not null && attempt.Status == QuizAttemptStatus.InProgress)
        {
            var deadline = attempt.StartedAt + quiz.TimeInMinutes;
            if (now >= deadline)
            {
                await QuizFinalizer.FinalizeAttempt(attempt, quiz, unitOfWork, ct);
                return Result<QuizTakingDto>.Error("انتهى وقت هذه المحاولة"); 

            }
        }

        if (attempt is not null && attempt.Status != QuizAttemptStatus.InProgress)
            return Result<QuizTakingDto>.Error("سبق أن قمت بتسليم هذا الاختبار");

        if (attempt is null)
        {
            if (quiz.DueDate.HasValue && quiz.DueDate < now)
                return Result<QuizTakingDto>.Error("انتهى موعد هذا الاختبار");

            attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                StudentId = studentId,
                StartedAt = now,
                Status = QuizAttemptStatus.InProgress
            };
            attemptRepo.Add(attempt);
            await unitOfWork.SaveChangesAsync(ct);
        }

        var savedAnswers = attempt.Answers.ToDictionary(a => a.QuestionId);

        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepo.GetByIdAsync(studentId, ct);

        var dto = new QuizTakingDto
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            Title = quiz.Title ?? string.Empty,
            TeacherName = student?.Teacher is not null
            ? $"{student.Teacher.FirstName} {student.Teacher.LastName}"
            : "أ. أحمد مصطفي",
            Subject = "اللغة الانجليزية",
            Instructions = quiz.Description ?? "لا يوجد تعليمات لهذا الإختبار",
            DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
            RemainingSeconds = Math.Max(0, (int)((attempt.StartedAt + quiz.TimeInMinutes - now).TotalSeconds)),
            Questions = quiz.Questions.Select(ql =>
            {
                var q = ql.Question;
                savedAnswers.TryGetValue(q.Id, out var saved);

                List<QuizChoiceDto>? choices = null;
                if (q is MCQQuestion mcq)
                {
                    choices = mcq.Choices.Select(c => new QuizChoiceDto
                    {
                        ChoiceId = c.Id,
                        Text = c.Text ?? string.Empty
                    }).ToList();
                }

                return new QuizQuestionTakingDto
                {
                    QuestionId = q.Id,
                    Text = q.Title,
                    Type = q.Type,
                    Degree = ql.Degree,
                    Choices = choices,
                    SelectedChoiceId = saved?.ChoiceId,
                    SavedTextAnswer = saved?.TextAnswer
                };
            }).ToList()
        };

        return Result<QuizTakingDto>.Success(dto);
    }
}