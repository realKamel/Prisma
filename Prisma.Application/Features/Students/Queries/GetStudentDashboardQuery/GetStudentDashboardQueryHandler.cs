using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;
using Prisma.Domain.Specifications.QuizAttemptSpecs;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;

public class GetStudentDashboardQueryHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    IStorageService storageService)
    : IRequestHandler<GetStudentDashboardQuery, Result<GetStudentDashboardResponse>>
{
    public async Task<Result<GetStudentDashboardResponse>> Handle(
        GetStudentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is not { } userId)
            return Result.Unauthorized("Login First");

        var user = await identityService.FindByIdAsync(userId);
        if (user is not Student)
            return Result.Error("Something went wrong");

        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<QuizAttempt, int>();

        var basicInfo = await studentRepo.FirstOrDefaultAsync(
            new StudentWithProjectionSpec<StudentBasicInfo>(userId, s =>
                new StudentBasicInfo(s.FirstName, s.AcademicYear!.Title, s.StreakDays)),
            cancellationToken);

        var enrollments = await enrollmentRepo.ListAsync(
            new EnrollmentWithProjectionSpec<EnrollmentDashboardInfo>(userId, e =>
                new EnrollmentDashboardInfo(
                    e.Id,
                    e.Lesson!.Id,
                    e.Lesson.Title,
                    e.Lesson.ImageThumbnailUrl,
                    e.Lesson.Duration,
                    e.Lesson.Teacher != null ? $"{e.Lesson.Teacher.FirstName} {e.Lesson.Teacher.SecondName}" : null,
                    e.Lesson.Teacher != null ? e.Lesson.Teacher.Subject : null,
                    e.IsCompleted,
                    e.ExpiresAt,
                    e.CreatedAt,
                    e.CompletedAt,
                    e.Lesson.Sections.Select(s => new SectionProgressInfo(
                        s.Duration,
                        s.Progresses.Any(p => p.StudentId == userId && p.IsCompleted)
                    )).ToList()
                )),
            cancellationToken);

        var score = await quizAttemptRepo.FirstOrDefaultAsync(
            new QuizAttemptWithProjectionSpec<TopQuizInfo>(userId, a => new TopQuizInfo(a.Degree,a.Quiz.TotalDegree)),
            cancellationToken);

        var topScore = score is not null ? (score.StudentScore / score.TopScore) * 100 : null;
        if (basicInfo is null)
            return Result.Error("Something went wrong");

        var studentDto = new StudentDto
        {
            FirstName = basicInfo.FirstName ?? string.Empty,
            GradeLabel = basicInfo.AcademicYearTitle ?? string.Empty,
        };

        var streakDto = new StreakDto { Count = basicInfo.StreakDays };

        int completedLessonsCount = enrollments.Count(e => e.IsCompleted);

        int totalStudyMinutes = enrollments
            .SelectMany(e => e.Sections)
            .Where(s => s.IsCompletedByStudent)
            .Sum(s => (int)s.Duration.TotalMinutes);

        var statsDto = new StatsDto
        {
            PurchasedLessons = enrollments.Count,
            CompletedLessons = completedLessonsCount,
            StudyHours = (int)Math.Ceiling(totalStudyMinutes / 60.0),
            TopQuizScore = topScore.HasValue ? (int)topScore.Value : 0,
        };

        var lastActiveEnrollment = enrollments
            .Where(e => !e.IsCompleted)
            .OrderByDescending(e => e.Sections.Any(s => s.IsCompletedByStudent))
            .ThenByDescending(e => e.CreatedAt)
            .FirstOrDefault();

        if (lastActiveEnrollment is null && enrollments.Any())
        {
            lastActiveEnrollment = enrollments
                .OrderByDescending(e => e.CompletedAt ?? e.CreatedAt)
                .FirstOrDefault();
        }

        NextLessonDto? nextLessonDto = null;

        if (lastActiveEnrollment is not null)
        {
            var completedSectionsCount = lastActiveEnrollment.Sections.Count(s => s.IsCompletedByStudent);
            var totalSections = lastActiveEnrollment.Sections.Count;

            var currentChapter = completedSectionsCount < totalSections
                ? completedSectionsCount + 1
                : totalSections;

            nextLessonDto = new NextLessonDto
            {
                Id = lastActiveEnrollment.LessonId.ToString(),
                Title = lastActiveEnrollment.LessonTitle ?? string.Empty,
                Subject = lastActiveEnrollment.Subject ?? string.Empty,
                TeacherName = lastActiveEnrollment.TeacherName ?? string.Empty,
                TeacherInitial = !string.IsNullOrEmpty(lastActiveEnrollment.TeacherName)
                    ? lastActiveEnrollment.TeacherName[0].ToString()
                    : string.Empty,
                CurrentChapter = currentChapter,
                TotalChapters = totalSections,
                PosterUrl = lastActiveEnrollment.LessonThumbnailUrl != null
                    ? await storageService.GetDownloadUrlAsync(storageService.DefaultBucketName, lastActiveEnrollment.LessonThumbnailUrl)
                    : string.Empty
            };
        }

        var lessonTasks = enrollments.Select(async enrollment =>
        {
            var now = DateTimeOffset.UtcNow;
            var status = ResolveStatus(enrollment, now);

            return new LessonCardDto
            {
                Id = enrollment.LessonId.ToString(),
                Title = enrollment.LessonTitle ?? string.Empty,
                Subject = enrollment.Subject ?? string.Empty,
                TeacherName = enrollment.TeacherName ?? string.Empty,
                TeacherInitial = !string.IsNullOrEmpty(enrollment.TeacherName)
                    ? enrollment.TeacherName[0].ToString()
                    : string.Empty,
                Duration = enrollment.LessonDuration,
                PosterUrl = enrollment.LessonThumbnailUrl != null
                    ? await storageService.GetDownloadUrlAsync(storageService.DefaultBucketName, enrollment.LessonThumbnailUrl)
                    : string.Empty,
                Status = status.ToString().ToLower(),
                ExpiresInDays = status == LessonStatus.Warn && enrollment.ExpiresAt.HasValue
                    ? (int)(enrollment.ExpiresAt.Value - now).TotalDays
                    : null,
            };
        });
        var lessons = (await Task.WhenAll(lessonTasks)).ToList();

        return Result<GetStudentDashboardResponse>.Success(new GetStudentDashboardResponse
        {
            Student = studentDto,
            Streak = streakDto,
            NextLesson = nextLessonDto,
            Lessons = lessons,
            Stats = statsDto,
        });
    }

    private static LessonStatus ResolveStatus(EnrollmentDashboardInfo enrollment, DateTimeOffset now)
    {
        if (enrollment.IsCompleted)
            return LessonStatus.Done;

        if (enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value <= now)
            return LessonStatus.Expired;

        if (enrollment.ExpiresAt.HasValue && (enrollment.ExpiresAt.Value - now).TotalDays < 3)
            return LessonStatus.Warn;

        if (enrollment.Sections.Any(s => s.IsCompletedByStudent))
            return LessonStatus.Progress;

        return LessonStatus.New;
    }
    public sealed record StudentBasicInfo(string? FirstName, string? AcademicYearTitle, int StreakDays);

    public sealed record SectionProgressInfo(TimeSpan Duration, bool IsCompletedByStudent);
    public sealed record TopQuizInfo(decimal? StudentScore, decimal? TopScore);

    public sealed record EnrollmentDashboardInfo(
        int EnrollmentId,
        int LessonId,
        string? LessonTitle,
        string? LessonThumbnailUrl,
        TimeSpan LessonDuration,
        string? TeacherName,   
        string? Subject,       
        bool IsCompleted,
        DateTimeOffset? ExpiresAt,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? CompletedAt,
        List<SectionProgressInfo> Sections
    );
}