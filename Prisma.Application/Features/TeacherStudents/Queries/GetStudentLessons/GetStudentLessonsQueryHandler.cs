using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentLessons;

public class GetStudentLessonsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetStudentLessonsQuery, List<StudentLessonDto>>
{
    public async Task<List<StudentLessonDto>> Handle(GetStudentLessonsQuery request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Domain.Entities.EnrollmentAggregate.Enrollment, int>();
        var enrollments = await enrollmentRepo.ListAsync(
            new EnrollmentsByStudentSpec(request.StudentId), cancellationToken);

        var result = new List<StudentLessonDto>();
        foreach (var e in enrollments)
        {
            var lesson = e.Lesson;
            if (lesson is null) continue;

            var progress = e.IsCompleted ? 100 :
                (e.Student?.SectionProgresses?.Count(sp => sp.Section?.LessonId == lesson.Id) ?? 0) > 0 ? 68 : 0;

            var status = e.IsCompleted ? "مكتمل" :
                         progress > 0 ? "في التقدم" : "جديد";

            var statusColor = status == "مكتمل" ? "bg-[rgba(78,203,141,0.16)] text-[var(--mint)]" :
                              status == "في التقدم" ? "bg-[rgba(147,112,219,0.12)] text-[var(--purple-lt)]" :
                              "bg-[var(--surface2)] text-[var(--muted)]";

            var progressColor = status == "مكتمل" ? "bg-[var(--mint)]" : "bg-[var(--purple)]";

            result.Add(new StudentLessonDto(
                lesson.Id,
                lesson.Title ?? "—",
                e.EnrollmentMethod == EnrollmentMethod.TeacherGrant ? "مُنح" : "اشتراك ذاتي",
                e.EnrollmentMethod == EnrollmentMethod.TeacherGrant ? "المعلمة" : "—",
                status,
                progress,
                statusColor,
                progressColor));
        }

        return result;
    }
}