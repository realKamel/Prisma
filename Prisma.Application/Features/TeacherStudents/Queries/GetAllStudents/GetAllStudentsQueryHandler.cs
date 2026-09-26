using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetAllStudents;

public class GetAllStudentsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IIdentityService identityService
) : IRequestHandler<GetAllStudentsQuery, Result<List<StudentListItemDto>>>
{
    public async Task<Result<List<StudentListItemDto>>> Handle(
        GetAllStudentsQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null)
        {
            return Result.NotFound();
        }

        if (user is Assistant assistant)
        {
            if (assistant.TeacherId is null)
                return Result.Unauthorized("Assistant is not associated with a teacher.");
            userId = assistant.TeacherId;
        }

        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var students = await studentRepo.ListAsync(
            new StudentsByTeacherSpec<StudentInfo>(
                userId.Value,
                s => new StudentInfo(
                    s.Id,
                    s.FirstName,
                    s.SecondName,
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
                            e.Lesson != null ? e.Lesson.Title : null
                        ))
                        .ToList(),
                    s.QuizAttempts.Select(q => new QuizAttemptInfo(q.Degree, q.Quiz.TotalDegree, q.CreatedAt)).ToList()
                )
            ),
            cancellationToken
        );

        var result = new List<StudentListItemDto>();
        foreach (var student in students)
        {
            var avgQuiz = student.QuizAttempts.Count != 0
                ? (int)student.QuizAttempts.Average(q => (q.Degree / q.TotalDegree) * 100)
                : 0;

            var active = student.Enrollments.Any(e =>
                e.Status == Domain.Enums.EnrollmentStatus.Active
            );

            var lastQuiz = student
                .QuizAttempts.OrderByDescending(q => q.CreatedAt)
                .FirstOrDefault();

            var lastEnrollment = student
                .Enrollments.OrderByDescending(e => e.CreatedAt)
                .FirstOrDefault();

            DateTimeOffset lastActivity = DateTimeOffset.UtcNow;

            if (lastQuiz != null || lastEnrollment != null)
            {
                lastActivity = new[] { lastQuiz?.CreatedAt, lastEnrollment?.CreatedAt }.Max().Value;
            }

            var lessonTitles = student
                .Enrollments.Where(e => e.LessonTitle != null)
                .Select(e => e.LessonTitle!)
                .Distinct()
                .ToList();

            var fullName =
                $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}".Trim();

            result.Add(
                new StudentListItemDto(
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
                    lessonTitles
                )
            );
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

    public record QuizAttemptInfo(decimal Degree, decimal TotalDegree, DateTimeOffset? CreatedAt);
}