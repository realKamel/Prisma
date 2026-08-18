using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

public record GetTeacherCatalogQuery(string? Search, PaginationParams Pagination)
    : IRequest<Result<PaginatedList<TeacherDto>>>;

public record TeacherDto(
    string Id,
    string FirstName,
    string SecondName,
    string Subject,
    int LessonsCount,
    bool Featured,
    string? ImageUrl,
    IReadOnlyList<AcademicYearDto> AcademicYears
);

public record AcademicYearDto(string Id, string Name);
