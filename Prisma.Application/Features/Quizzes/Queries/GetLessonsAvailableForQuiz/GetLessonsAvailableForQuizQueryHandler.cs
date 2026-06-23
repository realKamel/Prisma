using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Quizzes;

namespace Prisma.Application.Features.Quizzes.Queries.GetLessonsAvailableForQuiz;

public class GetLessonsAvailableForQuizQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLessonsAvailableForQuizQuery, Result<List<LessonOptionDto>>>
{
    public async Task<Result<List<LessonOptionDto>>> Handle(GetLessonsAvailableForQuizQuery request, CancellationToken ct)
    {
        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var lessons = await lessonRepo.ListAsync(new LessonsAvailableForQuizSpecification(), ct);

        var result = lessons.Select(l => new LessonOptionDto
        {
            LessonId = l.Id,
            Title = l.Title ?? string.Empty
        }).ToList();

        return Result<List<LessonOptionDto>>.Success(result);
    }
}
