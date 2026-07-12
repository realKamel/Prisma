using MediatR;
using Prisma.Application.Common.Responses;
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
            FirstOrDefaultAsync(new QuizByIdForDeleteSpecification(request.QuizId), ct);

        if (quiz is null)
            return Result.Failure("الاختبار غير موجود");

        if (quiz.Attempts.Any(a => a.Status != QuizAttemptStatus.InProgress))
            return Result.Failure(
                "مينفعش تحذف/ي اختبار عنده محاولات مسلمة أو متصححة");

        quiz.IsDeleted = true;
        quiz.DeletedAt = DateTimeOffset.UtcNow;


        // Unlink from lesson (one-to-one) before soft delete
        if (quiz.Scope == QuizScope.LessonQuiz && quiz.LessonId.HasValue)
        {
            var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
            var lesson = await lessonRepo.FirstOrDefaultAsync(
                new LessonByIdSpecification(quiz.LessonId.Value), ct);

            if (lesson is not null)
                lesson.QuizId = null;
        }

        
        quizRepo.Update(quiz);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success("تم حذف الاختبار بنجاح");
    }
}
