using Ardalis.Specification;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

public class GetStudentByIdQueryHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentHistoryQuery, Result<GetStudentHistoryResponse>>
{
    public class StudentWithEnrollmentHistorySpecification : Specification<Student>
    {
        public StudentWithEnrollmentHistorySpecification(string email)
        {
            Query.Where(s => s.Email == email)
                .Include(e => e.Enrollments)
                .ThenInclude(e => e.Lesson)
                .ThenInclude(l => l.Sections)
                .ThenInclude(l => l.Progresses)
                .Include(e => e.Enrollments)
                .ThenInclude(e => e.Lesson)
                .ThenInclude(l => l.Quizzes)
                .AsNoTracking(true);
        }
    }

    public async Task<Result<GetStudentHistoryResponse>> Handle(GetStudentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        //TODO: i should make dynamic
        var email = currentUserService.Email;
        email = "student3@prisma.edu.eg";
        if (email is null)
        {
            throw new UnauthorizedException("Login First");
        }

        var repo = unitOfWork.GetOrCreateRepository<Student, Guid>();

        var result = await repo
            .FirstOrDefaultAsync(new StudentWithEnrollmentHistorySpecification(email),
                cancellationToken);
        if (result is null)
        {
            throw new NotFoundException("Student", email);
        }

        var totalPurchasedCount = result.Enrollments.Count;
        var totalHours = TimeSpan.Zero;
        
        foreach (var enrollment in result.Enrollments)
        {
            totalHours = totalHours.Add(enrollment?.Lesson.Duration ?? TimeSpan.Zero);
        }

        var totalCompletedLesson = result.Enrollments.Count(e => e.IsCompleted);
        var avgQuizDegree = result.Enrollments.Where(e => e.Lesson.Quizzes.Count > 0)
            .Average(e => e.Lesson?.Quizzes.ElementAt(0).TotalDegree ?? 0);
        var status = new Status(totalPurchasedCount, totalCompletedLesson, (int)totalHours.TotalHours, avgQuizDegree);

        var history = result.Enrollments
            .Where(e => e.Lesson is not null)
            .Select(l =>
            {
                var lessonPercentage = (l.Lesson?.Sections.Sum(s => s.Progresses.Count(e => e.IsCompleted)) /
                    l.Lesson?.Sections.Sum(s => s.Progresses.Count) ?? 1);
                return new History(l.LessonId ?? 0, l.Lesson.ImageThumbnailUrl, l.Lesson.Title, l.Status.ToString(),
                    l.CreatedAt,
                    l.CompletedAt, l.ExpiresAt,
                    l.Lesson?.Quizzes.Count > 0 ? l.Lesson?.Quizzes.ElementAt(0).TotalDegree : 0, lessonPercentage);
            })
            .ToList();
        var response = new GetStudentHistoryResponse(status, history);
        return Result<GetStudentHistoryResponse>.Success(response);
    }
}