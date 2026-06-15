using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Exceptions;
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
            throw new UnauthorizedException("User is not authenticated.");

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonStatusSpecification(request.id);
        var lesson = await lessonRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            throw new NotFoundException("Lesson", request.id.ToString());

        var enrollment = lesson.Enrollments.FirstOrDefault(e => e.StudentId == userId);

        LessonCatalogStatus status;

        if (enrollment is null)
        {
            if (lesson.Prerequisite is not null)
            {
                var prereqEnrollment = lesson.Prerequisite.Enrollments
                    .FirstOrDefault(e => e.StudentId == userId);

                status = prereqEnrollment is { IsCompleted: false }
                    ? LessonCatalogStatus.Locked
                    : LessonCatalogStatus.Available;
            }
            else
            {
                status = LessonCatalogStatus.Available;
            }
        }
        else if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            status = LessonCatalogStatus.Expired;
        }
        else
        {
            status = LessonCatalogStatus.Purchased;
        }

        return Result<LessonStatusResponse>.Success(new LessonStatusResponse { Status = status });
    }
}
