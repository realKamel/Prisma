using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

internal class GetTeacherCatalogQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService
) : IRequestHandler<GetTeacherCatalogQuery, Result<TeacherCatalogResponse>>
{
    public async Task<Result<TeacherCatalogResponse>> Handle(
        GetTeacherCatalogQuery request,
        CancellationToken cancellationToken
    )
    {
        var studentCurrentYear = await GetStudentCurrentYearAsync(cancellationToken);

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
                    x.IsFeatured,
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

        return new TeacherCatalogResponse(
            teachers,
            filteredTeacherCount,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            studentCurrentYear
        );
    }

    private async Task<AcademicYearDto?> GetStudentCurrentYearAsync(CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return null;

        var user = await identityService.FindByIdAsync(userId.Value, ct);

        if (user is not Student { AcademicYearId: { } academicYearId })
            return null;
        var year = await unitOfWork
            .GetOrCreateRepository<AcademicYear, int>()
            .GetByIdAsync(academicYearId, ct);

        return year is null ? null : new AcademicYearDto(year.PublicId.ToString(), year.Title);
    }
}