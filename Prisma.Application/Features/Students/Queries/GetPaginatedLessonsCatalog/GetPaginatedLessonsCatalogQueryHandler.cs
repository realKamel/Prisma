using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Application.Features.Students.Queries.Helpers;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;

namespace Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;

internal sealed class GetPaginatedLessonsCatalogQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IStorageService storageService
) : IRequestHandler<GetPaginatedLessonsCatalogQuery, Result<PaginatedList<LessonCatalogDto>>>
{
    public async Task<Result<PaginatedList<LessonCatalogDto>>> Handle(
        GetPaginatedLessonsCatalogQuery request,
        CancellationToken cancellationToken
    )
    {
        if (currentUser.UserId is null)
        {
            return Result.Unauthorized();
        }

        var studentId = currentUser.UserId.Value;

        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var student = await studentRepo.GetByIdAsync(studentId, cancellationToken);

        if (student is null)
        {
            return Result.Unauthorized();
        }

        if (student.AcademicYearId is null)
        {
            return Result.NotFound($"Student {studentId} has no academic year assigned.");
        }

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var lessons = await enrollmentRepo.ListAsync(
            new ActiveEnrollmentsSpec(studentId, request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize),
            cancellationToken
        );

        var totalCount = await enrollmentRepo.CountAsync(
            new ActiveEnrollmentsSpec(studentId, request.PaginationParams.PageNumber,
                request.PaginationParams.PageSize),
            cancellationToken
        );


        var result = (
                await Task.WhenAll(lessons.Select(lesson =>
                    LessonMappingHelper.MapLesson(storageService, lesson, student)))
            )
            .OfType<LessonCatalogDto>().ToList();

        var paginatedResult = new PaginatedList<LessonCatalogDto>(result, totalCount,
            request.PaginationParams.PageNumber,
            request.PaginationParams.PageSize);

        return paginatedResult;
    }
}