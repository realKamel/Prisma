using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonMaterial;

public record GetLessonMaterialQuery(int LessonId) : IQuery<Result<List<LessonMaterialDto>>>;

public record LessonMaterialDto(int Id, string Title, long Size, string Type, DateTimeOffset? Date);
