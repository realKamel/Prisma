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
        var spec = new LessonStatusSpecification(request.id);
        var lesson = await lessonRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            return Result.NotFound($"Lesson with id '{request.id.ToString()}' was not found");

        var enrollment = lesson.Enrollments.FirstOrDefault(e => e.StudentId == userId);

        if (enrollment is null)
            return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Available });

        if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTimeOffset.UtcNow)
            return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Expired });

        if (lesson.Prerequisite is not null)
        {
            var prereqEnrollment = lesson.Prerequisite.Enrollments.FirstOrDefault(e => e.StudentId == userId);

            if (prereqEnrollment is { IsCompleted: false })
                return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Locked });
        }

        return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = LessonCatalogStatus.Purchased });
    }
}
