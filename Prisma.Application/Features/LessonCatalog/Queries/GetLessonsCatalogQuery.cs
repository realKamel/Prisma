using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public sealed record GetLessonsCatalogQuery
    : IRequest<Result<ICollection<LessonCatalogDto>>>;