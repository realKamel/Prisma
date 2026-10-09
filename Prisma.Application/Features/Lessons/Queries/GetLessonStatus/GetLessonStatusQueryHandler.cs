using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

internal sealed class GetLessonStatusQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetLessonStatusQuery, Result<LessonStatusResponse>>
{
    public async Task<Result<LessonStatusResponse>> Handle(
        GetLessonStatusQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid? userId = currentUserService.UserId;

        if (userId is null)
            return Result.Unauthorized();

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonStatusSpecification(request.LessonId, userId.Value);
        var lesson = await lessonRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
        {
            return Result.NotFound($"Lesson with id '{request.LessonId}' was not found");
        }

        if (!lesson.HasEnrollment)
        {
            return Result<LessonStatusResponse>.Success(
                new LessonStatusResponse(Status: EnrollmentStatus.Available)
            );
        }

        if (
            lesson.EnrollmentExpiresAt.HasValue
            && lesson.EnrollmentExpiresAt.Value < DateTimeOffset.UtcNow
        )
        {
            return Result<LessonStatusResponse>.Success(
                new LessonStatusResponse(Status: EnrollmentStatus.Expired)
            );
        }

        if (lesson is { HasPrerequisite: true, IsPrerequisiteCompleted: false })
        {
            return Result<LessonStatusResponse>.Success(
                new LessonStatusResponse(Status: EnrollmentStatus.Locked)
            );
        }

        return Result<LessonStatusResponse>.Success(
            new LessonStatusResponse(Status: EnrollmentStatus.Active)
        );
    }
}
