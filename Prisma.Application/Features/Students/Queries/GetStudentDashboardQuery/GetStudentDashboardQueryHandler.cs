using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        const string teacher = "أ. أحمد مصطفى";
        const string subject = "لغه انجليزيه";

        var studentDto = new StudentDto
        {
            FirstName = student.FirstName ?? string.Empty,
            GradeLabel = student.AcademicYear?.Title ?? string.Empty,
        };

        var streakDto = new StreakDto { Count = student.StreakDays };

        int completedLessonsCount = student.Enrollments.Count(e => e.IsCompleted);

        int totalStudyMinutes = 0;
        foreach (var enrollment in student.Enrollments)
        {
            totalStudyMinutes += enrollment.Lesson?.Sections
                .Where(s => s.Progresses.Any(p => p.StudentId == student.Id && p.IsCompleted))
                .Sum(s => (int)s.Duration.TotalMinutes) ?? 0;
        }

        var statsDto = new StatsDto
        {
            PurchasedLessons = student.Enrollments.Count,
            CompletedLessons = completedLessonsCount,

            StudyHours = (int)Math.Ceiling(totalStudyMinutes / 60.0),

            TopQuizScore = student.QuizAttempts.Any()
                ? (int)student.QuizAttempts.Max(a => a.Degree)
                : 0,
        };

        var lastActiveEnrollment = student.Enrollments
            .Where(e => !e.IsCompleted)
            .OrderByDescending(e => e.Lesson!.Sections
                .SelectMany(s => s.Progresses)
                .Any(p => p.StudentId == student.Id && p.IsCompleted))
            .ThenByDescending(e => e.CreatedAt)
            .FirstOrDefault();

        if (lastActiveEnrollment is null && student.Enrollments.Any())
        {
            lastActiveEnrollment = student.Enrollments
                .OrderByDescending(e => e.CompletedAt ?? e.CreatedAt)
                .FirstOrDefault();
        }

        NextLessonDto? nextLessonDto = null;

        if (lastActiveEnrollment?.Lesson is not null)
        {
            var lesson = lastActiveEnrollment.Lesson;

            var studentCompletedSections = lesson.Sections
                .Where(s => s.Progresses.Any(p => p.StudentId == student.Id && p.IsCompleted))
                .ToList();

            var currentChapter = studentCompletedSections.Count < lesson.Sections.Count
                ? studentCompletedSections.Count + 1
                : lesson.Sections.Count;

            nextLessonDto = new NextLessonDto
            {
                Id = lesson.Id.ToString(),
                Title = lesson.Title ?? string.Empty,
                Subject = subject,
                TeacherName = teacher,
                TeacherInitial = teacher[0].ToString(),
                CurrentChapter = currentChapter,
                TotalChapters = lesson.Sections.Count,
                PosterUrl = lesson.ImageThumbnailUrl ?? string.Empty
            };
        }

        var lessons = student.Enrollments.Select(enrollment =>
        {
            var lesson = enrollment.Lesson!;
            var now = DateTimeOffset.UtcNow;

            var status = ResolveStatus(enrollment, student.Id, now);

            return new LessonCardDto
            {
                Id = lesson.Id.ToString(),
                Title = lesson.Title ?? string.Empty,
                Subject = subject,
                TeacherName = teacher,
                TeacherInitial = teacher[0].ToString(),

                Duration = lesson.Duration,

                PosterUrl = lesson.ImageThumbnailUrl ?? string.Empty,
                Status = status.ToString().ToLower(),
                ExpiresInDays = status == LessonStatus.Warn && enrollment.ExpiresAt.HasValue
                                     ? (int)(enrollment.ExpiresAt.Value - now).TotalDays
                                     : null,
            };
        }).ToList();

        return Result<GetStudentDashboardResponse>.Success(new GetStudentDashboardResponse
        {
            Student = studentDto,
            Streak = streakDto,
            NextLesson = nextLessonDto,
            Lessons = lessons,
            Stats = statsDto,
        });
    }

    private static LessonStatus ResolveStatus(Enrollment enrollment, Guid studentId, DateTimeOffset now)
    {
        if (enrollment.IsCompleted)
            return LessonStatus.Done;

        if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value <= now)
            return LessonStatus.Expired;

        if (enrollment.ExpiresAt.HasValue && (enrollment.ExpiresAt.Value - now).TotalDays < 3)
            return LessonStatus.Warn;

        var hasAnyProgress = enrollment.Lesson?.Sections
            .SelectMany(s => s.Progresses)
            .Any(p => p.StudentId == studentId && p.IsCompleted) ?? false;

        if (hasAnyProgress)
            return LessonStatus.Progress;

        return LessonStatus.New;
    }
}