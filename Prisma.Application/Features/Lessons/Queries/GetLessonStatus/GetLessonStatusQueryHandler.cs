using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonStatus;

public class GetLessonStatusQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
    ) : IRequestHandler<GetLessonStatusQuery, Result<LessonStatusResponse>>
{
    public async Task<Result<LessonStatusResponse>> Handle(GetLessonStatusQuery request, CancellationToken cancellationToken)
    {
        Guid? userId = currentUserService.UserId;

        if (userId is null)
            return Result.Unauthorized("User is not authenticated.");

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonStatusSpecification(request.id, userId.Value);
        var lesson = await lessonRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            return Result.NotFound($"Lesson with id '{request.id}' was not found");

        if (!lesson.HasEnrollment)
            return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Available });

        if (lesson.EnrollmentExpiresAt.HasValue && lesson.EnrollmentExpiresAt.Value < DateTimeOffset.UtcNow)
            return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Expired });

        if (lesson.HasPrerequisite && !lesson.IsPrerequisiteCompleted)
            return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Locked });

        return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Purchased });
    }
}