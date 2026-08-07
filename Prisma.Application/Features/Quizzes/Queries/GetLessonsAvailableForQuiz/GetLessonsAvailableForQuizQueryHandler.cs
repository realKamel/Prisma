using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Application.Features.Quizzes.Specifications;

namespace Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;

public class GetLessonsAvailableForQuizQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLessonsAvailableForQuizQuery, Result<List<LessonOptionDto>>>
{
    public async Task<Result<List<LessonOptionDto>>> Handle(GetLessonsAvailableForQuizQuery request, CancellationToken ct)
    {
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();

        var lessons = await lessonRepo.ListAsync(
            new LessonsAvailableForQuizSpecification(), ct);

        return Result<List<LessonOptionDto>>.Success(lessons);
    }
}
