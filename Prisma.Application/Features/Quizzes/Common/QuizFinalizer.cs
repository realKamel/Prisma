using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Common;

public static class QuizFinalizer
{
    public static async Task FinalizeAttempt(QuizAttempt attempt, Quiz quiz, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        var answersByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);
        decimal totalScore = 0;
        var hasPendingWritten = false;

        foreach (var ql in quiz.Questions)
        {
            var question = ql.Question;
            answersByQuestion.TryGetValue(question.Id, out var answer);

            if (question is MCQQuestion mcq)
            {
                if (answer is null)
                {
                    
                    continue;
                }

                var selectedChoice = mcq.Choices.FirstOrDefault(c => c.Id == answer.ChoiceId);
                answer.IsCorrect = selectedChoice?.IsCorrect ?? false;
                answer.Score = answer.IsCorrect == true ? ql.Degree : 0;
                totalScore += answer.Score.Value;
            }
            else // WrittenQuestion
            {
                if (answer is null || answer.Score is null)
                    hasPendingWritten = true;
                else
                    totalScore += answer.Score.Value;
            }
        }

        attempt.SubmittedAt = DateTimeOffset.UtcNow;

        if (hasPendingWritten)
        {
            attempt.Status = QuizAttemptStatus.Submitted;
        }
        else
        {
            attempt.Status = QuizAttemptStatus.Graded;
            attempt.Degree = totalScore;

            if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
                await MarkEnrollmentCompleted(attempt.StudentId, quiz.LessonId.Value, unitOfWork, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public static async Task FinalizeAfterManualGrading(QuizAttempt attempt, Quiz quiz, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        // يُستدعى بعد تصحيح المعلم/المساعد لكل الأسئلة الكتابية
        var stillPending = attempt.Answers.Any(a => a.Score is null);
        if (stillPending) return;

        attempt.Status = QuizAttemptStatus.Graded;
        attempt.Degree = attempt.Answers.Sum(a => a.Score ?? 0);

        if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
            await MarkEnrollmentCompleted(attempt.StudentId, quiz.LessonId.Value, unitOfWork, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }

    private static async Task MarkEnrollmentCompleted(Guid studentId, int lessonId, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var enrollment = await enrollmentRepo.FirstOrDefaultAsync(
        new StudentLessonEnrollmentSpecification(studentId, lessonId),ct);

        if (enrollment is { IsCompleted: false })
        {
            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}
