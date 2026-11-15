using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.ObjectStorage;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Students.Queries.GetTeacherCatalog;

internal class GetTeacherCatalogQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService,
    IStorageService storageService
) : IRequestHandler<GetTeacherCatalogQuery, Result<TeacherCatalogResponse>>
{
    private const int AvatarLinkLifetimeInMinutes = 24 * 60;

    // What the query reads per teacher. The picture comes as bucket + object key; the download
    // link is built below, the same way as on the teacher's own profile page.
    private sealed record TeacherRow(
        string Id,
        string FirstName,
        string SecondName,
        string Subject,
        int LessonsCount,
        bool IsFeatured,
        string? BucketName,
        string? ObjectKey,
        List<AcademicYearDto> AcademicYears
    );

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

        var rows = await repo.ListAsync(
            new PagedTeacherWithDetailsSpec<TeacherRow>(
                request.Search,
                x => new TeacherRow(
                    x.Id.ToString(),
                    x.FirstName,
                    x.SecondName,
                    x.Subject,
                    x.Lessons.Count,
                    x.IsFeatured,
                    x.ProfileImageFile == null
                        ? null
                        : x.ProfileImageFile.ObjectStorage.BucketName,
                    x.ProfileImageFile == null ? null : x.ProfileImageFile.ObjectKey,
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

        var teachers = new List<TeacherDto>(rows.Count);
        foreach (var row in rows)
        {
            var avatarUrl =
                row.BucketName is null || row.ObjectKey is null
                    ? null
                    : await storageService.GetDownloadUrlAsync(
                        row.BucketName,
                        row.ObjectKey,
                        AvatarLinkLifetimeInMinutes
                    );

            teachers.Add(
                new TeacherDto(
                    row.Id,
                    row.FirstName,
                    row.SecondName,
                    row.Subject,
                    row.LessonsCount,
                    row.IsFeatured,
                    avatarUrl,
                    row.AcademicYears
                )
            );
        }

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