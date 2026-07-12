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
        var totalSecurityEvents = attempt.TabSwitchCount + attempt.CopyPasteAttemptCount;
        var hasSecurityViolation = totalSecurityEvents > 0;

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

        if (hasPendingWritten)
        {
            attempt.Status = QuizAttemptStatus.Submitted;
        }
        else if (hasSecurityViolation)
        {
            // Auto-graded but held for security review
            // Save the computed score so teacher can apply penalty on top of it
            attempt.Degree = totalScore;
            attempt.Status = QuizAttemptStatus.Submitted;
        }
        else{
            attempt.Degree = totalScore;
            attempt.Status = QuizAttemptStatus.Graded;

            if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
                await MarkEnrollmentCompleted(attempt.StudentId, quiz.LessonId.Value, unitOfWork, ct);
        }

        attempt.SubmittedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
    }

    public static async Task FinalizeAfterManualGrading(QuizAttempt attempt, Quiz quiz, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        // Check if all written answers are graded
        var stillPending = attempt.Answers.Any(a => a.Score is null);
        if (stillPending) return;

        attempt.Degree = attempt.Answers.Sum(a => a.Score ?? 0);

        var totalSecurityEvents = attempt.TabSwitchCount + attempt.CopyPasteAttemptCount;
        var hasSecurityViolation = totalSecurityEvents > 0;

        if (hasSecurityViolation)
        {
            // Keep as Submitted — teacher still needs to review and apply penalty
            attempt.Status = QuizAttemptStatus.Submitted;
        }
        else
        {
            // No security issues — fully graded
            attempt.Status = QuizAttemptStatus.Graded;
            if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
                await MarkEnrollmentCompleted(attempt.StudentId, quiz.LessonId.Value, unitOfWork, ct);
        }

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
