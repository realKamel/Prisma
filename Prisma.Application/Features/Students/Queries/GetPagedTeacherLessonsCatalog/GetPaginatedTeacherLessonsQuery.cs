using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.GetLessonsCatalog;

namespace Prisma.Application.Features.Students.Queries.GetPagedTeacherLessonsCatalog;

public record GetPaginatedTeacherLessonsQuery(Guid Id, string? Keyword, PaginationParams Pagination)
    : IRequest<Result<PaginatedList<LessonCatalogDto>>>;
