using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Prisma.Application.Features.Students.Queries.GetTeacherCatalog;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Queries.GetPublicTeacherProfile;

internal class GetPublicTeacherProfileQueryHandler(IUnitOfWork unitOfWork, HybridCache hybridCache)
    : IRequestHandler<GetPublicTeacherProfileQuery, Result<PublicTeacherProfileResponse>>
{
    public async Task<Result<PublicTeacherProfileResponse>> Handle(
        GetPublicTeacherProfileQuery request,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"public_teacher_profile_{request.Id}";

        //Cache the underlying DTO, passing request.Id as state to avoid allocations
        var response = await hybridCache.GetOrCreateAsync(
            cacheKey,
            request.Id,
            async (teacherId, ct) =>
            {
                var teacherRepo = unitOfWork.GetOrCreateRepository<Teacher, Guid>();
                var teacher = await teacherRepo.FirstOrDefaultAsync(
                    new TeacherWithDetailsSpecification(teacherId),
                    ct
                );

                if (teacher is null)
                {
                    return null; // Don't wrap in Result here
                }

                return new PublicTeacherProfileResponse(
                    teacher.Id,
                    teacher.FirstName,
                    teacher.SecondName,
                    teacher.Subject,
                    Bio: null,
                    teacher.TeacherAvatarUrl,
                    teacher.Lessons.Count,
                    teacher.TeacherStudents.Count,
                    YearsOfExperience: null,
                    [
                        .. teacher.AcademicYears.Select(ay => new AcademicYearDto(
                            Id: ay.Id.ToString(),
                            Name: ay.AcademicYear.Title
                        )),
                    ],
                    Featured: false
                );
            },
            cancellationToken: cancellationToken
        );

        // Result outcome after cache retrieval
        if (response is null)
        {
            return Result.NotFound();
        }

        return response;
    }
}
