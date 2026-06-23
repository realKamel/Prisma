using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;
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
            return Result.Failure("Quiz not Found");

        if (quiz.Attempts.Count > 0)
            return Result.Failure(
                "Cannot delete quiz because students have already attempted it.");

        quiz.IsDeleted = true;
        quiz.DeletedAt = DateTimeOffset.UtcNow;


        foreach (var questionLink in quiz.Questions)
        {
            questionLink.IsDeleted = true;
            questionLink.DeletedAt = DateTimeOffset.UtcNow;
        }

        quizRepo.Update(quiz);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
