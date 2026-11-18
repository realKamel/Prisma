using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.QuizAttemptSpecs;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Features.Students.Queries.GetStudentStatsHistoryQuery;

internal sealed class GetStudentStatsHistoryQueryHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : IRequestHandler<GetStudentStatsHistoryQuery, Result<StatsHistoryDto>>
{
    private const int HistoryDays = 60;

    public async Task<Result<StatsHistoryDto>> Handle(
        GetStudentStatsHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        if (currentUserService.UserId is not { } userId)
            return Result.Unauthorized();

        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();

        var enrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithProjectionSpec<EnrollmentHistoryInfo>(
                userId,
                e => new EnrollmentHistoryInfo(
                    e.CreatedAt,
                    e.IsCompleted,
                    e.CompletedAt,
                    e.Lesson!.Sections.Select(s => new SectionHistoryInfo(
                            s.TimeDuration,
                            s.Progresses
                                .Where(p => p.StudentId == userId)
                                .Select(p => p.IsCompleted)
                                .FirstOrDefault(),
                            s.Progresses
                                .Where(p => p.StudentId == userId)
                                .Select(p => p.WatchedSeconds)
                                .FirstOrDefault(),
                            s.Progresses
                                .Where(p => p.StudentId == userId)
                                .Select(p => p.UpdatedAt)
                                .FirstOrDefault()
                        ))
                        .ToList()
                )
            ),
            cancellationToken
        );

        var attempts = await quizAttemptRepo.ListAsync(
            new QuizAttemptWithProjectionSpec<QuizHistoryInfo>(
                userId,
                a => new QuizHistoryInfo(a.Degree, a.Quiz.TotalDegree, a.CreatedAt)
            ),
            cancellationToken
        );

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var windowStart = today.AddDays(-(HistoryDays - 1));

        var response = new StatsHistoryDto
        {
            PurchasedLessons = BuildDailySeries(
                enrollments
                    .Where(e => e.CreatedAt.HasValue)
                    .Select(e => (e.CreatedAt!.Value, 1d)),
                v => v.Sum(),
                today
            ),
            CompletedLessons = BuildDailySeries(
                enrollments
                    .Where(e => e.IsCompleted && e.CompletedAt.HasValue)
                    .Select(e => (e.CompletedAt!.Value, 1d)),
                v => v.Sum(),
                today
            ),
            StudyHours = BuildDailySeries(
                enrollments
                    .SelectMany(e => e.Sections)
                    .Where(s => s.LastActivityAt.HasValue)
                    .Select(s =>
                    {
                        var durationSeconds = s.Duration?.Seconds ?? 0;
                        var seconds = s.IsCompleted
                            ? durationSeconds
                            : Math.Min(s.WatchedSeconds, durationSeconds);
                        return (s.LastActivityAt!.Value, seconds / 60.0);
                    }),
                v => Math.Round(v.Sum()),
                today
            ),
            TopQuizScore = attempts
                .Where(a => a.CreatedAt.HasValue && a.Score.HasValue && a.TotalDegree is > 0)
                .Select(a => (
                    Day: DateOnly.FromDateTime(a.CreatedAt!.Value.UtcDateTime),
                    At: a.CreatedAt!.Value,
                    Percent: Math.Round((double)(a.Score!.Value / a.TotalDegree!.Value * 100), 1)
                ))
                .Where(a => a.Day >= windowStart)
                .OrderBy(a => a.At)
                .Select(a => new StatPointDto(a.Day, a.Percent))
                .ToList(),
        };

        return Result<StatsHistoryDto>.Success(response);
    }

    /// <summary>Buckets events per UTC day and fills the last 60 days (empty days = 0).</summary>
    private static List<StatPointDto> BuildDailySeries(
        IEnumerable<(DateTimeOffset At, double Value)> events,
        Func<IEnumerable<double>, double> aggregate,
        DateOnly today
    )
    {
        var byDay = events
            .GroupBy(e => DateOnly.FromDateTime(e.At.UtcDateTime))
            .ToDictionary(g => g.Key, g => aggregate(g.Select(e => e.Value)));

        return Enumerable
            .Range(0, HistoryDays)
            .Select(i => today.AddDays(i - (HistoryDays - 1)))
            .Select(day => new StatPointDto(day, byDay.GetValueOrDefault(day)))
            .ToList();
    }

    public sealed record SectionHistoryInfo(
        TimeDuration? Duration, 
        bool IsCompleted,
        double WatchedSeconds, 
        DateTimeOffset? LastActivityAt);

    public sealed record EnrollmentHistoryInfo(
        DateTimeOffset? CreatedAt,
        bool IsCompleted,
        DateTimeOffset? CompletedAt,
        List<SectionHistoryInfo> Sections
    );

    public sealed record QuizHistoryInfo(
        decimal? Score,
        decimal? TotalDegree,
        DateTimeOffset? CreatedAt
    );
}