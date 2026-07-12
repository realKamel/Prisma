using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Common;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.GradeWrittenAnswers;

public class GradeWrittenAnswersCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GradeWrittenAnswersCommand, Result<GradeWrittenAnswersResultDto>>
{
    public async Task<Result<GradeWrittenAnswersResultDto>> Handle(GradeWrittenAnswersCommand request, CancellationToken ct)
    {
        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();
        var attempt = await attemptRepo.FirstOrDefaultAsync(
            new AttemptWithAnswersAndQuizSpecification(request.AttemptId), ct);

        if (attempt is null)
            return Result<GradeWrittenAnswersResultDto>.Failure("المحاولة غير موجودة");

        if (attempt.Status == QuizAttemptStatus.InProgress)
            return Result<GradeWrittenAnswersResultDto>.Failure("الطالب لسه في الاختبار");

        if (attempt.Status == QuizAttemptStatus.Graded)
            return Result<GradeWrittenAnswersResultDto>.Failure("المحاولة دي متصححة بالفعل");

        // Build answer lookup for quick access
        var answersById = attempt.Answers.ToDictionary(a => a.Id);

        foreach (var grade in request.Grades)
        {
            if (!answersById.TryGetValue(grade.AnswerId, out var answer))
                return Result<GradeWrittenAnswersResultDto>.Failure($"الإجابة رقم {grade.AnswerId} غير موجودة في هذه المحاولة");

            // Only written answers should be graded manually
            if (answer.ChoiceId.HasValue)
                return Result<GradeWrittenAnswersResultDto>.Failure($"الإجابة رقم {grade.AnswerId} MCQ ومش محتاجة تصحيح يدوي");

            // Validate score doesn't exceed question max degree
            var questionLink = attempt.Quiz.Questions
                .FirstOrDefault(ql => ql.QuestionId == answer.QuestionId);

            if (questionLink is not null && grade.Score > questionLink.Degree)
                return Result<GradeWrittenAnswersResultDto>.Failure($"الدرجة المدخلة أكبر من الدرجة الكاملة للسؤال ({questionLink.Degree})");

            answer.Score = grade.Score;
            answer.IsCorrect = grade.Score > 0;
        }

        await unitOfWork.SaveChangesAsync(ct);

        // Check if all written answers are now graded, then finalize
        await QuizFinalizer.FinalizeAfterManualGrading(attempt, attempt.Quiz, unitOfWork, ct);

        var totalSecurityEvents = attempt.TabSwitchCount + attempt.CopyPasteAttemptCount;

        return Result<GradeWrittenAnswersResultDto>.Success(
            new GradeWrittenAnswersResultDto
            {
                Status = attempt.Status == QuizAttemptStatus.Graded ? "graded" : "submitted",
                HeldForSecurityReview = attempt.Status == QuizAttemptStatus.Submitted
                                        && totalSecurityEvents > 0
                                        && attempt.Answers.All(a => a.Score != null)
            },
            "تم حفظ التصحيح بنجاح"
        );
    }
}
