using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;

public record GetLessonMaterialQuery(int LessonId) : IRequest<Result<List<LessonMaterialDto>>>;

public record LessonMaterialDto(
    int Id,
    string Title,
    string Size,
    string Type,
    string Date
);