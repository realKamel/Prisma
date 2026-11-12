using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Students.Queries.GetTeacherCatalog;
 
public record GetTeacherCatalogQuery(string? Search, PaginationParams Pagination)
    : IRequest<Result<TeacherCatalogResponse>>;

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


public record TeacherCatalogResponse : PaginatedList<TeacherDto>
{
    public TeacherCatalogResponse(
        IReadOnlyCollection<TeacherDto> items,
        int count,
        int pageNumber,
        int pageSize,
        AcademicYearDto? studentCurrentYear
    )
        : base(items, count, pageNumber, pageSize)
    {
        StudentCurrentYear = studentCurrentYear;
    }

    public AcademicYearDto? StudentCurrentYear { get; }
}