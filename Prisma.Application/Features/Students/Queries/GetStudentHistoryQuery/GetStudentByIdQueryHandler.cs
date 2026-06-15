using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Exceptions;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Students.Queries.GetStudentHistoryQuery;

public class GetStudentByIdQueryHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentHistoryQuery, Result<GetStudentHistoryResponse>>
{
    public async Task<Result<GetStudentHistoryResponse>> Handle(GetStudentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        //TODO: i should make it dynamic
        var email = currentUserService.Email;
        // email = "student2@prisma.edu.eg";
        // email = "mmm@gmail.com";

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

        var userId = result.Id;

        var totalPurchasedCount = result.Enrollments.Count;

        var totalHours = result.Enrollments.Aggregate(TimeSpan.Zero,
            (current, enrollment) => current.Add(enrollment.Lesson?.Duration ?? TimeSpan.Zero));

        var totalCompletedLesson = result.Enrollments.Count(e => e.IsCompleted);

        var avgQuizDegree = result.Enrollments.Average(e => e.Lesson?.Quiz?.TotalDegree ?? 0);

        var status = new Status(totalPurchasedCount, totalCompletedLesson, (int)totalHours.TotalHours,
            (int)avgQuizDegree);

        List<History> history = new(result.Enrollments.Count);

        foreach (var item in result.Enrollments)
        {
            var totalSections = item.Lesson?.Sections.Count ?? 1;

            var completedSections = item.Lesson?
                .Sections
                .Select(s => s.Progresses
                    .Where(p => p.StudentId == userId)
                    .Select(p => p.Percentage)
                    .FirstOrDefault()
                ) // 0 if no progress row yet
                .Sum();

            var lessonProgress = item.IsCompleted ? 100 :
                completedSections == 0 ? 0 : (completedSections / totalSections) ?? 0;

            var entry = new History(item.LessonId ?? 0,
                item.Lesson?.ImageThumbnailUrl,
                item.Lesson?.Title ?? "",
                item.Status.ToString(),
                item.CreatedAt,
                item.CompletedAt,
                item.ExpiresAt,
                item.Lesson?.Quiz?.TotalDegree ?? 0,
                lessonProgress);

            history.Add(entry);
        }

        var response = new GetStudentHistoryResponse(status, history);
        return Result<GetStudentHistoryResponse>.Success(response);
    }
}