using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;

public sealed record GetPaginatedLessonsCatalogQuery(PaginationParams PaginationParams)
    : IRequest<Result<PaginatedList<LessonCatalogDto>>>;