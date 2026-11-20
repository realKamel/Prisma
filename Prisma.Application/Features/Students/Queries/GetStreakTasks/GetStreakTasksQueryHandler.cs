using Ardalis.Result;
using Ardalis.Specification;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.QuizAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Application.Features.Students.Common;
using Prisma.Application.Common.Time;

namespace Prisma.Application.Features.Students.Queries.GetStreakTasks;

public sealed class GetStreakTasksQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IQueryHandler<GetStreakTasksQuery, Result<StreakTasksDto>>
{
    public async Task<Result<StreakTasksDto>> Handle(GetStreakTasksQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            return Result<StreakTasksDto>.Unauthorized();

        var student = await unitOfWork
            .GetOrCreateRepository<Student, Guid>()
            .GetByIdAsync(userId.Value, ct);
        if (student is null)
            return Result<StreakTasksDto>.NotFound();

        var today = CairoDate.Today();
        var tasks = new List<StreakTaskDto>();

        if (student.LastPracticedDate != today && await PracticePool.HasPoolAsync(unitOfWork, student.Id, ct))
            tasks.Add(new("practice", null, null, string.Empty));

        var quiz = await unitOfWork
            .GetOrCreateRepository<Quiz, int>()
            .FirstOrDefaultAsync(new NextQuizForStreakSpec(student.Id, DateTimeOffset.UtcNow), ct);
                if (quiz is not null)
                    tasks.Add(quiz.LessonGuid is { } lessonGuid
                        ? new StreakTaskDto("lessonQuiz", null, lessonGuid, quiz.Title)
                        : new StreakTaskDto("quiz", quiz.Id, null, quiz.Title));

        var assignment = await unitOfWork
            .GetOrCreateRepository<Assignment, int>()
            .FirstOrDefaultAsync(new NextAssignmentForStreakSpec(student.Id, DateTimeOffset.UtcNow), ct);
        if (assignment is not null)
            tasks.Add(new("assignment", null, assignment.LessonId, assignment.LessonTitle));

        return Result.Success(new StreakTasksDto(student.LastStreakDate == today, tasks));
    }
    internal record NextQuizInfo(int Id, string Title, Guid? LessonGuid);

    internal class NextQuizForStreakSpec : Specification<Quiz, NextQuizInfo>
    {
        public NextQuizForStreakSpec(Guid studentId, DateTimeOffset now)
        {
            Query
                .Where(q => (q.AvailableFrom == null || q.AvailableFrom <= now)
                    && (q.DueDate == null || q.DueDate >= now)
                    && ((q.AcademicYear != null && q.AcademicYear.Students.Any(s => s.Id == studentId))
                        || (q.Lesson != null && q.Lesson.Enrollments.Any(e => e.StudentId == studentId)))
                    && !q.Attempts.Any(a => a.StudentId == studentId))
                .OrderBy(q => q.Id)
                .Take(1)
                .Select(q => new NextQuizInfo(
                    q.Id,
                    q.Title,
                    q.Lesson != null ? q.Lesson.PublicId : null));
        }
    }
    internal record NextAssignmentInfo(Guid LessonId, string LessonTitle);
    internal class NextAssignmentForStreakSpec : Specification<Assignment, NextAssignmentInfo>
    {
        public NextAssignmentForStreakSpec(Guid studentId, DateTimeOffset now)
        {
            Query
                .Where(a => a.Lesson.Enrollments.Any(e => e.StudentId == studentId) 
                &&  a.DueDate > now 
                && !a.Submissions.Any(s => s.StudentId == studentId))
                .OrderBy(a => a.DueDate)
                .Take(1)
                .Select(a => new NextAssignmentInfo(a.Lesson.PublicId, a.Lesson.Title ?? string.Empty));
        }
    }
}