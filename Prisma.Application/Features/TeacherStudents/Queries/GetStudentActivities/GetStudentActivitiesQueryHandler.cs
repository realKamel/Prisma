using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentActivities;

internal sealed class GetStudentActivitiesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentActivitiesQuery, List<StudentActivityDto>>
{
    public async Task<List<StudentActivityDto>> Handle(GetStudentActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.EnrollmentAggregate.Enrollment, int>();
        var quizAttemptRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.QuizAggregate.QuizAttempt, int>();
        var sectionProgressRepo =
            unitOfWork.GetOrCreateRepository<Domain.Entities.LessonAggregate.SectionProgress, int>();

        var activities = new List<StudentActivityDto>();

        var enrollments = await enrollmentRepo.ListAsync(
            new EnrollmentsByStudentForActivitySpec(request.StudentId), cancellationToken);
        foreach (var e in enrollments)
        {
            if (e.CreatedAt.HasValue)
            {
                activities.Add(new StudentActivityDto(
                    $"اشترى درس {e.Lesson?.Title ?? "—"}",
                    e.CreatedAt.Value,
                    "bg-[var(--border)]"));
            }
        }

        var quizAttempts = await quizAttemptRepo.ListAsync(
            new QuizAttemptsByStudentSpec(request.StudentId), cancellationToken);
        foreach (var q in quizAttempts)
        {
            if (q.CreatedAt.HasValue)
            {
                activities.Add(new StudentActivityDto(
                    $"سلّم كويز {q.Quiz?.Title ?? "—"} — نتيجة {(int)(q.Degree / q.Quiz.TotalDegree * 100)}%",
                    q.CreatedAt.Value,
                    "bg-[var(--star)]"));
            }
        }

        var sectionProgresses = await sectionProgressRepo.ListAsync(
            new SectionProgressByStudentSpec(request.StudentId), cancellationToken);
        foreach (var sp in sectionProgresses)
        {
            if (sp.CreatedAt.HasValue)
            {
                activities.Add(new StudentActivityDto(
                    $"أكمل درس {sp.Section?.Lesson?.Title ?? "—"}",
                    sp.CreatedAt.Value,
                    "bg-[var(--mint)]"));
            }
        }

        return activities.OrderByDescending(a => a.Time).Take(10).ToList();
    }
}