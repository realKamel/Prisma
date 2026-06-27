using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentDetails;

public class GetStudentDetailsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetStudentDetailsQuery, StudentListItemDto?>
{
    public async Task<StudentListItemDto?> Handle(GetStudentDetailsQuery request, CancellationToken cancellationToken)
    {
        var studentRepo = unitOfWork.GetOrCreateRepository<Student, Guid>();
        var student = await studentRepo.FirstOrDefaultAsync(
            new StudentByIdWithDetailsSpec(request.StudentId), cancellationToken);

        if (student is null)
            throw new NotFoundException("Student", request.StudentId);

        var enrollments = student.Enrollments?.ToList() ?? new List<Domain.Entities.EnrollmentAggregate.Enrollment>();
        var quizAttempts = student.QuizAttempts?.ToList() ?? new List<Domain.Entities.QuizAggregate.QuizAttempt>();
        var avgQuiz = quizAttempts.Any() ? (int)quizAttempts.Average(q => q.Degree) : 0;
        var active = enrollments.Any(e => e.Status == Domain.Enums.EnrollmentStatus.Active);

        var lessonTitles = enrollments
            .Where(e => e.Lesson?.Title != null)
            .Select(e => e.Lesson!.Title!)
            .Distinct()
            .ToList();

        return new StudentListItemDto(
            student.Id,
            $"{student.FirstName} {student.SecondName} {student.ThirdName} {student.LastName}".Trim(),
            student.AcademicYear?.Title ?? "—",
            "—",
            enrollments.Count,
            avgQuiz,
            active,
            student.PhoneNumber,
            student.ParentPhoneNumber,
            lessonTitles);
    }
}