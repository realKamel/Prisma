using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public sealed record GetLessonsCatalogQuery
    : IRequest<Result<ICollection<LessonCatalogDto>>>;