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

        var (totalScore, hasPendingWritten) = CalculateScore(attempt, quiz);

        var hasSecurityViolation = attempt.TabSwitchCount > 0 || attempt.CopyPasteAttemptCount > 0;


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
        else
        {
            attempt.Degree = totalScore;
            attempt.Status = QuizAttemptStatus.Graded;

            await CompleteLessonIfNeeded(
                attempt.StudentId,
                quiz,
                unitOfWork,
                ct);
        }

        attempt.SubmittedAt = DateTimeOffset.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
    }

    public static async Task FinalizeAfterManualGrading(QuizAttempt attempt, Quiz quiz, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        // Check if all written answers are graded
        if (attempt.Answers.Any(a => a.Score is null))
            return;


        attempt.Degree = attempt.Answers.Sum(a => a.Score ?? 0);

        var hasSecurityViolation =
            attempt.TabSwitchCount > 0 ||
            attempt.CopyPasteAttemptCount > 0;

        if (hasSecurityViolation)
        {
            // Keep as Submitted — teacher still needs to review and apply penalty
            attempt.Status = QuizAttemptStatus.Submitted;
        }
        else
        {
            // No security issues — fully graded
            attempt.Status = QuizAttemptStatus.Graded;

            await CompleteLessonIfNeeded(
                attempt.StudentId,
                quiz,
                unitOfWork,
                ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private static (decimal TotalScore, bool HasPendingWritten) CalculateScore(
        QuizAttempt attempt,
        Quiz quiz)
    {
        var answersByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);
        decimal totalScore = 0;
        bool hasPendingWritten = false;

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
        return (totalScore, hasPendingWritten);


    }

    private static async Task CompleteLessonIfNeeded(
        Guid studentId,
        Quiz quiz,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (quiz.Scope != QuizScope.LessonQuiz || !quiz.LessonId.HasValue)
            return;

        await MarkEnrollmentCompleted(
            studentId,
            quiz.LessonId.Value,
            unitOfWork,
            ct);
    }


    private static async Task MarkEnrollmentCompleted(Guid studentId, int lessonId, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var enrollment = await enrollmentRepo.FirstOrDefaultAsync(
        new StudentLessonEnrollmentSpecification(studentId, lessonId), ct);

        if (enrollment is { IsCompleted: false })
        {
            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}
