using MediatR;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Commands.DeleteQuiz;

public class DeleteQuizCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteQuizCommand, Result>
{
    public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken ct)
    {
        var quizRepo = unitOfWork.GetOrCreateRepository<Quiz, int>();

        var quiz = await quizRepo.
            FirstOrDefaultAsync(new QuizByIdSpecification(request.QuizId), ct);

        if (quiz is null)
            return Result.Error("الاختبار غير موجود");

        var attemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();

        var hasSubmittedAttempts = await attemptRepo.AnyAsync(
            new SubmittedAttemptsForQuizSpecification(request.QuizId), ct);

        if (hasSubmittedAttempts)
        {
            return Result.Error("مينفعش تحذف/ي اختبار عنده محاولات مسلمة أو متصححة");
        }

        quiz.IsDeleted = true;
        quiz.DeletedAt = DateTimeOffset.UtcNow;

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        // Unlink from lesson (one-to-one) before soft delete
        if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
        {
            var lesson = await lessonRepo.FirstOrDefaultAsync(
                new LessonByIdSpecification(quiz.LessonId.Value), ct);

            if (lesson is not null)
                lesson.QuizId = null;
        }

        
        //quizRepo.Update(quiz);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.SuccessWithMessage("تم حذف الاختبار بنجاح");
    }
}
