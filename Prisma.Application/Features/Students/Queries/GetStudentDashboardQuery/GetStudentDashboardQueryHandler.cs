using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;

public class GetStudentDashboardQueryHandler(
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentDashboardQuery, Result<GetStudentDashboardResponse>>
{
    public async Task<Result<GetStudentDashboardResponse>> Handle(
        GetStudentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Login First");

        var repo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var student = await repo.FirstOrDefaultAsync(
            new StudentDashboardSpecification(userId), cancellationToken)
            ?? throw new BadRequestException("Something went wrong");

        // ── Student ──────────────────────────────────────────────
        var studentDto = new StudentDto
        {
            FirstName  = student.FirstName,
            GradeLabel = student.AcademicYear?.Title ?? string.Empty,
        };

        // ── Streak ───────────────────────────────────────────────
        var streakDto = new StreakDto { Count = student.StreakDays };

        // ── Stats ────────────────────────────────────────────────
        var allSections = student.Enrollments
            .SelectMany(e => e.Lesson.Sections)
            .ToList();

        var statsDto = new StatsDto
        {
            PurchasedLessons  = student.Enrollments.Count,
            CompletedLessons  = student.Enrollments.Count(e => e.IsCompleted),
            StudyHours        = (int)allSections
                                    .Where(s => s.Progresses.Any(p => p.IsCompleted))
                                    .Sum(s => s.Duration.TotalMinutes / 60),
            TopQuizScore      = student.QuizAttempts.Count > 0
                                    ? (int)student.QuizAttempts.Max(a => a.Degree)
                                    : 0,
        };

        // ── Next Lesson ───────────────────────────────────────────
        // Find the enrollment whose lesson has the most recent completed section.
        // Falls back to the last incomplete enrollment ordered by creation if none started.
        var lastActiveEnrollment = student.Enrollments
            .Where(e => !e.IsCompleted)
            .OrderByDescending(e => e.Lesson.Sections
                .SelectMany(s => s.Progresses)
                .Where(p => p.IsCompleted)
                .Any())
            .ThenByDescending(e => e.CreatedAt)
            .FirstOrDefault();

        NextLessonDto? nextLessonDto = null;

        if (lastActiveEnrollment is not null)
        {
            var lesson = lastActiveEnrollment.Lesson;

            var completedSections = lesson.Sections
                .Where(s => s.Progresses.Any(p => p.IsCompleted))
                .ToList();

            // current chapter = last completed section's SortOrder (1-based display)
            // if nothing completed yet, show 0 completed out of total
            var currentChapter = completedSections.Count > 0
                ? completedSections.Max(s => s.SortOrder)
                : 0;

            var progressPercent = lesson.Sections.Count > 0
                ? (int)Math.Round(completedSections.Count * 100.0 / lesson.Sections.Count)
                : 0;

            nextLessonDto = new NextLessonDto
            {
                Id              = lesson.Id.ToString(),
                Title           = lesson.Title,
                PosterUrl       = lesson.ImageThumbnailUrl,
                CurrentChapter  = currentChapter,
                TotalChapters   = lesson.Sections.Count,
            };
        }

        // ── Lesson Cards ──────────────────────────────────────────
        var lessons = student.Enrollments.Select(enrollment =>
        {
            var lesson = enrollment.Lesson;
            var now    = DateTimeOffset.UtcNow;

            var status = ResolveStatus(enrollment, now);

            return new LessonCardDto
            {
                Id             = lesson.Id.ToString(),
                Title          = lesson.Title,
                DurationLabel  = FormatDuration(lesson.Duration),
                PosterUrl      = lesson.ImageThumbnailUrl,
                Status         = status,
                ExpiresInDays  = status == LessonStatus.Warn
                                     ? (int)(enrollment.ExpiresAt!.Value - now).TotalDays
                                     : null,
            };
        }).ToList();

        return Result<GetStudentDashboardResponse>.Success(new GetStudentDashboardResponse
        {
            Student    = studentDto,
            Streak     = streakDto,
            NextLesson = nextLessonDto,
            Lessons    = lessons,
            Stats      = statsDto,
        });
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    private static LessonStatus ResolveStatus(Enrollment enrollment, DateTimeOffset now)
    {
        if (enrollment.IsCompleted)
            return LessonStatus.Done;

        if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value <= now)
            return LessonStatus.Expired;

        if (enrollment.ExpiresAt.HasValue && (enrollment.ExpiresAt.Value - now).TotalDays < 3)
            return LessonStatus.Warn;

        if (enrollment.Lesson.Sections.SelectMany(s => s.Progresses).Any(p => p.IsCompleted))
            return LessonStatus.Progress;

        return LessonStatus.New;
    }

    /// <summary>
    /// Formats a TimeSpan duration into Arabic e.g. "٤ ساعات", "٣٠ دقيقة".
    /// </summary>
    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            var hours = (int)Math.Round(duration.TotalHours);
            return $"{hours.ToArabicNumerals()} {(hours == 1 ? "ساعة" : "ساعات")}";
        }

        var minutes = (int)Math.Round(duration.TotalMinutes);
        return $"{minutes.ToArabicNumerals()} دقيقة";
    }
}

// ── Small extension to convert digits to Eastern Arabic numerals ──
file static class ArabicNumeralsExtensions
{
    private static readonly char[] ArabicDigits = ['٠','١','٢','٣','٤','٥','٦','٧','٨','٩'];

    public static string ToArabicNumerals(this int n)
        => string.Concat(n.ToString().Select(c => char.IsDigit(c) ? ArabicDigits[c - '0'] : c));
}