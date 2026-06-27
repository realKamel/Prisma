using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllStudentsQuery, List<StudentListItemDto>>
{
    private static readonly Guid TeacherId = Guid.Parse("b90a811d-98a4-4353-81a5-cc75e32699b2");

    public async Task<List<StudentListItemDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var students = await studentRepo.ListAsync(new StudentsByTeacherSpec(TeacherId), cancellationToken);

        var result = new List<StudentListItemDto>();
        foreach (var student in students)
        {
            var enrollments  = student.Enrollments?.ToList()   ?? new List<Domain.Entities.EnrollmentAggregate.Enrollment>();
            var quizAttempts = student.QuizAttempts?.ToList()  ?? new List<Domain.Entities.QuizAggregate.QuizAttempt>();

            var avgQuiz = quizAttempts.Any() ? (int)quizAttempts.Average(q => q.Degree) : 0;
            var active  = enrollments.Any(e => e.Status == Domain.Enums.EnrollmentStatus.Active);

            var lastActivity = "—";
            var lastQuiz       = quizAttempts.OrderByDescending(q => q.CreatedAt).FirstOrDefault();
            var lastEnrollment = enrollments.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
            if (lastQuiz != null || lastEnrollment != null)
            {
                var latest = new[] { lastQuiz?.CreatedAt, lastEnrollment?.CreatedAt }.Max();
                if (latest.HasValue)
                {
                    var diff = DateTimeOffset.UtcNow - latest.Value;
                    lastActivity = diff.TotalMinutes < 1  ? "الآن"           :
                                   diff.TotalHours   < 1  ? $"منذ {diff.Minutes} د" :
                                   diff.TotalDays    < 1  ? $"منذ {diff.Hours} س"   :
                                   diff.TotalDays    < 2  ? "منذ يوم"        :
                                   diff.TotalDays    < 7  ? $"منذ {diff.Days} أيام" :
                                   diff.TotalDays    < 14 ? "منذ أسبوع"      : "منذ فترة";
                }
            }

            var lessonTitles = enrollments
                .Where(e => e.Lesson?.Title != null)
                .Select(e => e.Lesson!.Title!)
                .Distinct()
                .ToList();

            var fullName = $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}".Trim();

            result.Add(new StudentListItemDto(
                student.Id,
                fullName,
                student.FirstName  ?? string.Empty,
                student.SecondName ?? string.Empty,
                student.ThirdName  ?? string.Empty,
                student.LastName   ?? string.Empty,
                student.Email      ?? string.Empty,
                student.AcademicYear?.Title ?? "—",
                student.AcademicYearId ?? 0,
                lastActivity,
                enrollments.Count,
                avgQuiz,
                active,
                student.PhoneNumber,
                student.ParentPhoneNumber,
                lessonTitles));
        }

        return result;
    }
}