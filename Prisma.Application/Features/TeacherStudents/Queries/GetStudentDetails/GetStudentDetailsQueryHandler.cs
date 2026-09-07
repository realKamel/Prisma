using MediatR;
using Ardalis.Result;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;

public class GetStudentDetailsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentDetailsQuery, Result<StudentListItemDto>>
{
    public async Task<Result<StudentListItemDto>> Handle(GetStudentDetailsQuery request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepo.FirstOrDefaultAsync(
            new StudentByIdWithDetailsSpec<StudentInfo>(request.StudentId, s => new StudentInfo(
              s.Id,
              s.FirstName,
              s.SecondName ,
              s.ThirdName,
              s.LastName ,
              s.Email ,
              s.PhoneNumber,
              s.ParentPhoneNumber,
              s.AcademicYear!.Title ,
              s.AcademicYearId ?? 0,
               s.Enrollments.Select(e => new EnrollmentInfo(
                            e.Status,
                            e.Lesson != null ? e.Lesson.Title : null
                        ))
                        .ToList(),
                    s.QuizAttempts.Select(q => new QuizAttemptInfo(q.Degree, q.Quiz!.TotalDegree)).ToList()
            )), cancellationToken);

        if (student is null)
            return Result.NotFound($"Student with id '{request.StudentId}' was not found");

        var enrollments = student.Enrollments ;
        var quizAttempts = student.QuizAttempts; ;

        var avgQuiz = quizAttempts.Any() ? (int)quizAttempts.Average(q => (q.Degree/q.TotalDegree) * 100) : 0;
        var active = enrollments.Any(e => e.Status == Domain.Enums.EnrollmentStatus.Active);

        var lessonTitles = enrollments
            .Where(e => e.LessonTitle != null)
            .Select(e => e.LessonTitle)
            .Distinct()
            .ToList();

        var fullName = $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}".Trim();

        return new StudentListItemDto(
            student.Id,
            fullName,
            student.FirstName ?? string.Empty,
            student.SecondName ?? string.Empty,
            student.ThirdName ?? string.Empty,
            student.LastName ?? string.Empty,
            student.Email ?? string.Empty,
            student.AcademicYearTitle ?? "—",
            student.AcademicYearId,
            "—",
            enrollments.Count,
            avgQuiz,
            active,
            student.PhoneNumber,
            student.ParentPhoneNumber,
            lessonTitles);
    }

    public record StudentInfo(
        Guid Id,
        string FirstName,
        string SecondName,
        string ThirdName,
        string LastName,
        string Email,
        string PhoneNumber,
        string ParentPhoneNumber,
        string AcademicYearTitle,
        int AcademicYearId,
        List<EnrollmentInfo> Enrollments,
        List<QuizAttemptInfo> QuizAttempts
        );

    public record EnrollmentInfo(
        Domain.Enums.EnrollmentStatus Status,
        string? LessonTitle
    );
    public record QuizAttemptInfo(decimal Degree,
        decimal TotalDegree
      );
}