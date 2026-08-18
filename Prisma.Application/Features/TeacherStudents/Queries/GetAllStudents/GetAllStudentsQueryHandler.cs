using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetAllStudentsQuery, List<StudentListItemDto>>
{
    public async Task<List<StudentListItemDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.UserId;
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var students = await studentRepo.ListAsync(
            new StudentsByTeacherSpec<StudentInfo>(currentUser.Value, s => new StudentInfo(
                s.Id,
                s.FirstName ?? string.Empty,
                s.SecondName ?? string.Empty,
                s.ThirdName ?? string.Empty,
                s.LastName ?? string.Empty,
                s.Email ?? string.Empty,
                s.PhoneNumber,
                s.ParentPhoneNumber,
                s.AcademicYear != null ? s.AcademicYear.Title : "—",
                s.AcademicYearId ?? 0,
                s.Enrollments.Select(e => new EnrollmentInfo(
                    e.Status,
                    e.CreatedAt,
                    e.Lesson != null ? e.Lesson.Title : null)).ToList(),
                s.QuizAttempts.Select(q => new QuizAttemptInfo(q.Degree, q.CreatedAt)).ToList()
            )),
            cancellationToken);

        var result = new List<StudentListItemDto>();
        foreach (var student in students)
        {
            var avgQuiz = student.QuizAttempts.Any() ? (int)student.QuizAttempts.Average(q => q.Degree) : 0;
            var active = student.Enrollments.Any(e => e.Status == Domain.Enums.EnrollmentStatus.Active);

            var lastActivity = "—";
            var lastQuiz = student.QuizAttempts.OrderByDescending(q => q.CreatedAt).FirstOrDefault();
            var lastEnrollment = student.Enrollments.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
            if (lastQuiz != null || lastEnrollment != null)
            {
                var latest = new[] { lastQuiz?.CreatedAt, lastEnrollment?.CreatedAt }.Max();
                if (latest.HasValue)
                {
                    var diff = DateTimeOffset.UtcNow - latest.Value;
                    lastActivity = diff.TotalMinutes < 1 ? "الآن" :
                                   diff.TotalHours < 1 ? $"منذ {diff.Minutes} د" :
                                   diff.TotalDays < 1 ? $"منذ {diff.Hours} س" :
                                   diff.TotalDays < 2 ? "منذ يوم" :
                                   diff.TotalDays < 7 ? $"منذ {diff.Days} أيام" :
                                   diff.TotalDays < 14 ? "منذ أسبوع" : "منذ فترة";
                }
            }

            var lessonTitles = student.Enrollments
                .Where(e => e.LessonTitle != null)
                .Select(e => e.LessonTitle!)
                .Distinct()
                .ToList();

            var fullName = $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}".Trim();

            result.Add(new StudentListItemDto(
                student.Id,
                fullName,
                student.FirstName,
                student.SecondName,
                student.ThirdName,
                student.LastName,
                student.Email,
                student.AcademicYearTitle,
                student.AcademicYearId,
                lastActivity,
                student.Enrollments.Count,
                avgQuiz,
                active,
                student.PhoneNumber,
                student.ParentPhoneNumber,
                lessonTitles));
        }

        return result;
    }

    public record StudentInfo(
        Guid Id,
        string FirstName,
        string SecondName,
        string ThirdName,
        string LastName,
        string Email,
        string? PhoneNumber,
        string? ParentPhoneNumber,
        string AcademicYearTitle,
        int AcademicYearId,
        List<EnrollmentInfo> Enrollments,
        List<QuizAttemptInfo> QuizAttempts
    );

    public record EnrollmentInfo(
        Domain.Enums.EnrollmentStatus Status,
        DateTimeOffset? CreatedAt,
        string? LessonTitle
    );

    public record QuizAttemptInfo(
        decimal Degree,
        DateTimeOffset? CreatedAt
    );
}