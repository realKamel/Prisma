using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;

namespace Prisma.Application.Features.Students.Queries.GetPaginatedTeacherLessonsCatalog;

public record GetPaginatedTeacherLessonsQuery(
    Guid TeacherId,
    string? Keyword,
    PaginationParams Pagination
) : IRequest<Result<PaginatedList<LessonCatalogDto>>>;
