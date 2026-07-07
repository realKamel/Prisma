using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
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
            throw new UnauthorizedException("User must be authenticated to access lesson player");

        var lessonRepo = unitOfWork.GetOrCreateRepository<Lesson, int>();
        var spec = new LessonPlayerWithDetailsSpecification(request.id);
        var lesson = await lessonRepo.FirstOrDefaultAsync(spec, cancellationToken);

        if (lesson is null)
            throw new NotFoundException("Lesson", request.id);

        var enrollment = lesson.Enrollments?.FirstOrDefault(e => e.StudentId == studentId.Value);
        var quiz = lesson.Quiz;
        var assignment = lesson.Assignment;

        const string teacher = "أ. أحمد مصطفى";
        const string subject = "لغه انجليزيه";

        var expiryDays = enrollment?.ExpiresAt is not null
            ? (int)(enrollment.ExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays
            : 0;
        var sections = new List<SectionDto>();
        foreach (var s in lesson.Sections ?? [])
        {
            var progress = s.Progresses?.FirstOrDefault(p => p.StudentId == studentId.Value);
            var contentUrl = s.PlaybackId != null
                ? await videoStorageService.GetVideoUrlAsync(s.PlaybackId)
                : null;

            sections.Add(new SectionDto
            {
                Id = s.SortOrder,
                SectionId = s.Id,
                Title = s.Title ?? string.Empty,
                Duration = s.Duration.ToString(@"hh\:mm\:ss"),
                IsCompleted = progress?.IsCompleted ?? false,
                ContentUrl = contentUrl,
                Progress = progress?.IsCompleted == true ? 100 : 0,
                WatchedSeconds = progress?.WatchedSeconds ?? 0
            });
        }
        
        var materials = new List<MaterialDto>();
        foreach (var m in lesson.LessonMaterials ?? [])
        {
            materials.Add(new MaterialDto
            {
                Title = m.Title ?? string.Empty,
                DownloadUrl = m.DownloadUrl != null
                    ? await storageService.GetDownloadUrlAsync("prisma", m.DownloadUrl)
                    : string.Empty,
                Type = (int)m.Type switch
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
            Outcomes = lesson.Outcomes?.ToList() ?? new List<string>(),
            Materials = materials,


            Quiz = quiz is null
                ? null
                : new QuizDto
                {
                    Id = quiz.Id,
                    QuestionsCount = quiz.Questions?.Count ?? 0,
                    DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
                    PassingScore = (int)quiz.TotalDegree / 2
                },

            Assignment = assignment is null
                ? null
                : new AssignmentDto
                {
                    Id = assignment.Id,
                    ContentURL = assignment.ContentURL != null ?
                    await storageService.GetDownloadUrlAsync("prisma", assignment.ContentURL) : string.Empty,
                    DueDate = assignment.DueDate.ToString("yyyy-MM-dd")
                },

            Sections = sections
        };

        return Result<LessonPlayerResult>.Success(result);
    }
}