using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Students.Queries.GetLessonsCatalog;

public sealed record GetLessonsCatalogQuery : IRequest<Result<ICollection<LessonCatalogDto>>>;
