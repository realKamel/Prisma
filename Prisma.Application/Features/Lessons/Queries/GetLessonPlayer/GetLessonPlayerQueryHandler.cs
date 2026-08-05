using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;

public class GetLessonPlayerQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IVideoStorageService videoStorageService,
    IStorageService storageService) : IRequestHandler<GetLessonPlayerQuery, Result<LessonPlayerResult>>
{
    public async Task<Result<LessonPlayerResult>> Handle(GetLessonPlayerQuery request,
        CancellationToken cancellationToken)
    {
        var studentId = currentUserService.UserId;
        if (studentId == null)
            return Result.Unauthorized("User must be authenticated to access lesson player");

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonPlayerWithDetailsSpecification(request.id, studentId.Value);
        var lesson = await lessonRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            return Result.NotFound($"Lesson with id '{request.id}' was not found");

        var teacher = lesson.TeacherName ?? string.Empty;
        var subject = lesson.Subject ?? string.Empty;

        var expiryDays = lesson.EnrollmentExpiresAt is not null
            ? (int)(lesson.EnrollmentExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays
            : 0;

        var sections = new List<SectionDto>();
        foreach (var s in lesson.Sections)
        {
            var contentUrl = s.PlaybackId != null
                ? await videoStorageService.GetVideoUrlAsync(s.PlaybackId)
                : null;

            sections.Add(new SectionDto
            {
                Id = s.SortOrder,
                SectionId = s.Id,
                Title = s.Title ?? string.Empty,
                Duration = s.Duration.ToString(@"hh\:mm\:ss"),
                IsCompleted = s.IsCompleted,
                ContentUrl = contentUrl,
                Progress = s.IsCompleted ? 100 : 0,
                WatchedSeconds = s.WatchedSeconds
            });
        }

        var materials = new List<MaterialDto>();
        foreach (var m in lesson.Materials)
        {
            materials.Add(new MaterialDto
            {
                Title = m.Title ?? string.Empty,
                DownloadUrl = m.DownloadUrl != null
                    ? await storageService.GetDownloadUrlAsync(storageService.DefaultBucketName, m.DownloadUrl)
                    : string.Empty,
                Type = m.Type switch
                {
                    0 => "pdf",
                    1 => "video",
                    2 => "audio",
                    _ => "unknown"
                }
            });
        }

        var result = new LessonPlayerResult
        {
            Id = lesson.Id,
            Title = lesson.Title ?? string.Empty,
            Category = $"{subject} · {lesson.Title}",
            Subject = subject,
            Description = lesson.Description ?? string.Empty,
            Teacher = teacher,
            VideoPoster = lesson.ImageThumbnailUrl ?? string.Empty,

            ValidityDays = expiryDays > 0 ? expiryDays : 30,
            Outcomes = lesson.Outcomes,
            Materials = materials,

            Quiz = lesson.Quiz is null ? null : new QuizDto
            {
                Id = lesson.Quiz.Id,
                QuestionsCount = lesson.Quiz.QuestionsCount,
                DurationMinutes = (int)lesson.Quiz.TimeInMinutes.TotalMinutes,
                PassingScore = (int)lesson.Quiz.TotalDegree,
                IsAttempted = lesson.Quiz.IsAttempted,
            },

            Assignment = lesson.Assignment is null
                ? null
                : new AssignmentDto
                {
                    Id = lesson.Assignment.Id,
                    ContentURL = lesson.Assignment.ContentURL != null
                        ? await storageService.GetDownloadUrlAsync(storageService.DefaultBucketName, lesson.Assignment.ContentURL)
                        : string.Empty,
                    DueDate = lesson.Assignment.DueDate.ToString("yyyy-MM-dd"),
                    FileName = lesson.Assignment.SubmissionTitle ?? string.Empty
                },

            Sections = sections
        };

        return Result<LessonPlayerResult>.Success(result);
    }
}