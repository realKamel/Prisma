using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Common;

public static class QuizFinalizer
{
    public static bool ApplyFinalization(QuizAttempt attempt, Quiz quiz, DateTimeOffset? submittedAt = null)
    {
        var (totalScore, hasPendingWritten) = CalculateScore(attempt, quiz);
        var hasSecurityViolation = attempt.TabSwitchCount > 0 || attempt.CopyPasteAttemptCount > 0;
        var fullyGraded = false;

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
            fullyGraded = true;
        }
        attempt.SubmittedAt = submittedAt ?? DateTimeOffset.UtcNow;
        return fullyGraded;


    }
    public static async Task FinalizeAttempt(QuizAttempt attempt, Quiz quiz, IUnitOfWork unitOfWork, CancellationToken ct, DateTimeOffset? submittedAt = null)
    {

        if (ApplyFinalization(attempt, quiz, submittedAt))
            await CompleteLessonIfNeeded(attempt.StudentId, quiz, unitOfWork, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }

    public static async Task CompleteLessonsInBulk(
        IReadOnlyCollection<(Guid StudentId, int LessonId)> pairs,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        if (pairs.Count == 0) return;

        var studentIds = pairs.Select(p => p.StudentId).Distinct().ToList();
        var lessonIds = pairs.Select(p => p.LessonId).Distinct().ToList();

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var enrollments = await enrollmentRepo.ListAsync(
            new IncompleteEnrollmentsSpecification(studentIds, lessonIds), ct);

        var wanted = pairs.ToHashSet();
        var now = DateTimeOffset.UtcNow;

        foreach (var e in enrollments.Where(e =>
             e.StudentId.HasValue && e.LessonId.HasValue &&
             wanted.Contains((e.StudentId.Value, e.LessonId.Value))))
        {
            e.IsCompleted = true;
            e.CompletedAt = now;
        }
    }

    public static async Task FinalizeAfterManualGrading(QuizAttempt attempt, Quiz quiz, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        // Check if all written answers are graded
        if (attempt.Answers.Any(a => a.Score is null))
        {
            return;
        }

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
            else if (question is WrittenQuestion) // WrittenQuestion
            {
                // A blank answer has nothing to grade: it scores 0 instead of waiting for a teacher
                if (answer is null || string.IsNullOrWhiteSpace(answer.TextAnswer))
                {
                    if (answer is not null)
                    {
                        answer.IsCorrect = false;
                        answer.Score = 0;
                    }
                    continue;
                }

                if (answer.Score is null)
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
        {
            return;
        }

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
            enrollment.Status = EnrollmentStatus.Done;
            enrollment.CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}
