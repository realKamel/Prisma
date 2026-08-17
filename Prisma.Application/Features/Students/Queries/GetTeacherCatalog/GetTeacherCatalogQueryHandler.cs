using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teacher;

namespace Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

internal class GetTeacherCatalogQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTeacherCatalogQuery, Result<PaginatedList<TeacherDto>>>
{
    public async Task<Result<PaginatedList<TeacherDto>>> Handle(
        GetTeacherCatalogQuery request,
        CancellationToken cancellationToken
    )
    {
        var repo = unitOfWork.GetOrCreateRepository<Teacher, Guid>();

        var filteredTeacherCount = await repo.CountAsync(
            new FilteredTeacherSpec(request.Search),
            cancellationToken
        );

        var teachers = await repo.ListAsync(
            new PagedTeacherWithDetailsSpec<TeacherDto>(
                request.Search,
                x => new TeacherDto(
                    x.Id.ToString(),
                    x.FirstName,
                    x.SecondName,
                    x.Subject,
                    x.Lessons.Count,
                    false,
                    x.TeacherAvatarUrl,
                    x.AcademicYears.Select(y => new AcademicYearDto(
                            y.AcademicYear.PublicId.ToString(),
                            y.AcademicYear.Title
                        ))
                        .ToList()
                ),
                request.Pagination.PageNumber,
                request.Pagination.PageSize
            ),
            cancellationToken
        );

        return new PaginatedList<TeacherDto>(
            teachers,
            filteredTeacherCount,
            request.Pagination.PageNumber,
            request.Pagination.PageSize
        );
    }
}
