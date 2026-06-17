using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Lessons;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;

public class GetLessonPlayerQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<GetLessonPlayerQuery, Result<LessonPlayerResult>>
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
            Materials = lesson.LessonMaterials?.Select(m => new MaterialDto
            {
                Title = m.Title ?? string.Empty,
                DownloadUrl = m.DownloadUrl ?? string.Empty,

                Type = ((int)m.Type) switch
                {
                    0 => "pdf",
                    1 => "video",
                    2 => "audio",
                    _ => "unknown"
                }
            }).ToList() ?? new List<MaterialDto>(),


            Quiz = quiz is null
                ? null
                : new QuizDto
                {
                    Id = quiz.Id,
                    QuestionsCount = quiz.Questions?.Count ?? 0,
                    DurationMinutes = (int)(quiz.TimeInMinutes.TotalMinutes),
                    PassingScore = 0
                },

            Assignment = assignment is null
                ? null
                : new AssignmentDto
                {
                    Id = assignment.Id,
                    ContentURL = assignment.ContentURL ?? string.Empty,
                    DueDate = assignment.DueDate.ToString("yyyy-MM-dd")
                },

            Sections = lesson.Sections?.Select(s =>
            {
                var progress = s.Progresses?.FirstOrDefault(p => p.StudentId == studentId.Value);
                return new SectionDto
                {
                    Id = s.SortOrder,
                    Title = s.Title ?? string.Empty,
                    Duration = s.Duration.ToString(@"hh\:mm\:ss"),
                    IsCompleted = progress?.IsCompleted ?? false,
                    ContentUrl = s.ContentURL,
                    Progress = progress?.IsCompleted == true ? 100 : 0
                };
            }).ToList() ?? new List<SectionDto>()
        };

        return Result<LessonPlayerResult>.Success(result);
    }
}