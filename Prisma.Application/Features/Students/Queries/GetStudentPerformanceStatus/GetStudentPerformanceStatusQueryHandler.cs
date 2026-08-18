using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Repositories;

namespace Prisma.Application.Features.Students.Queries.GetStudentPerformanceStatus;

internal class GetStudentPerformanceStatusQueryHandler(
    ICurrentUserService currentUserService,
    IEnrollmentRepository repository
) : IRequestHandler<GetStudentPerformanceStatusQuery, Result<StatusDto>>
{
    public record LessonWithIdDto(int? lessonId);

    public record SingleEnrollmentEntryDto(
        int? LessonId,
        bool IsCompletedLesson,
        TimeSpan? LessonDuration,
        decimal? AverageQuizDegree
    );

    public async Task<Result<StatusDto>> Handle(
        GetStudentPerformanceStatusQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        //var repo = unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var readModel = await repository.GetPerformanceStatsAsync(userId.Value, cancellationToken);

        if (readModel is null)
        {
            return new StatusDto(0, 0, 0, 0);
        }

        return new StatusDto(
            readModel.TotalLessons,
            readModel.CompletedLessons,
            readModel.TotalStudyHours,
            readModel.AverageQuizDegree
        );
    }
}
