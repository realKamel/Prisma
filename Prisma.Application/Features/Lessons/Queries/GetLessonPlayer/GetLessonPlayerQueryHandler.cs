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

        var enrollment = lesson.Enrollments.FirstOrDefault();
        var quiz = lesson.Quiz;
        var assignment = lesson.Assignment;

        const string teacher = "أ. فاطمة علي";
        const string subject = "فيزياء";

        var expiryDays = enrollment?.ExpiresAt is not null
            ? (int)(enrollment.ExpiresAt.Value - DateTimeOffset.UtcNow).TotalDays
            : 0;

        var sections = lesson.Sections.Select(s =>
        {
            var progress = s.Progresses.FirstOrDefault(p => p.StudentId == studentId.Value);
            return new SectionItem
            {
                Id = s.Id.ToString(),
                Title = s.Title,
                Type = "video",
                Duration = s.Duration.ToString(@"mm\:ss"),
                IsCompleted = progress?.IsCompleted ?? false,
                ContentUrl = s.ContentURL
            };
        }).ToList();

        var currentSection = sections.FirstOrDefault(s => !s.IsCompleted);

        var result = new LessonPlayerResult
        {
            Id = lesson.Id,
            Title = lesson.Title ?? string.Empty,
            Category = $"{subject} · {lesson.Title}",
            Subject = subject,
            ParentId = lesson.Id.ToString(),
            ParentTitle = lesson.Title ?? string.Empty,
            IsPurchased = enrollment is not null,
            PurchaseLabel = enrollment is not null ? "مشتري" : string.Empty,
            Teacher = teacher,
            TeacherInitial = teacher[0].ToString(),
            ExpiryLabel = $"صلاحية الدرس: {expiryDays} يوم",
            VideoPoster = lesson.ImageThumbnailUrl ?? string.Empty,
            AboutTab =
                new AboutTab
                {
                    Description = lesson.Description ?? string.Empty, Objectives = lesson.Outcomes.ToList()
                },
            Materials = new List<Material>(),
            Quiz =
                quiz is null
                    ? null
                    : new Quiz
                    {
                        Id = quiz.Id.ToString(),
                        QuestionsCount = quiz.Questions.Count,
                        DurationMinutes = (int)quiz.TimeInMinutes.TotalMinutes,
                        PassingScore = 0
                    },
            Assignment =
                assignment is null
                    ? null
                    : new Assignment
                    {
                        Id = assignment.Id.ToString(),
                        Title = "واجب الدرس",
                        DueDate = assignment.DueDate.ToString("yyyy-MM-dd")
                    },
            Sections = new List<SectionPlayer>
            {
                new SectionPlayer { Title = currentSection?.Title ?? string.Empty, Items = sections }
            }
        };

        return Result<LessonPlayerResult>.Success(result);
    }
}