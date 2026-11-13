using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonMaterialQuery;

public record GetLessonMaterialQuery(int LessonId) : IRequest<Result<List<LessonMaterialDto>>>;

public record LessonMaterialDto(int Id, string Title, long Size, string Type, string Date);
