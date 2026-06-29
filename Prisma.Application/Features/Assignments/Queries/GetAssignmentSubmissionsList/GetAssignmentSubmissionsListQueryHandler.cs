using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Assignments.Dtos;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Assignments;
using Prisma.Domain.Specifications.Users;

namespace Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionsList;

public class GetAssignmentSubmissionsListQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    : IRequestHandler<GetAssignmentSubmissionsListQuery, Result<AssignmentSubmissionsResponseDto>>
{
    // Grace period for grading lock — if grading started more than 30 min ago, consider it abandoned
    private static readonly TimeSpan GradingLockExpiry = TimeSpan.FromMinutes(30);

    public async Task<Result<AssignmentSubmissionsResponseDto>> Handle(GetAssignmentSubmissionsListQuery request, CancellationToken ct)
    {
        var currentUserId = currentUser.UserId!.Value;

        var assignmentRepo = unitOfWork.GetOrCreateRepository<Assignment, int>();
        var enrollmentRepo = unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var submissionRepo = unitOfWork.GetOrCreateRepository<AssignmentSubmission, int>();

        // 1. Fetch all assignments (filtered by lesson if provided)
        var assignments = await assignmentRepo.ListAsync(
            new AssignmentsWithLessonSpecification(), ct);

        if (request.LessonId.HasValue)
            assignments = assignments
                .Where(a => a.LessonId == request.LessonId.Value)
                .ToList();

        if (!assignments.Any())
            return new AssignmentSubmissionsResponseDto();

        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var lessonIds = assignments.Select(a => a.LessonId).Distinct().ToList();

        // 2. Single query for all enrollments across all relevant lessons
        var enrollments = await enrollmentRepo.ListAsync(
            new EnrolledStudentsByLessonIdsSpecification(lessonIds), ct);

        // 3. Single query for all submissions across all relevant assignments
        var submissions = await submissionRepo.ListAsync(
            new SubmissionsByAssignmentIdsSpecification(assignmentIds), ct);

        // Collect all GradingByUserIds that are not null
        var gradingUserIds = submissions
            .Where(s => s.IsBeingGraded && s.GradingByUserId.HasValue)
            .Select(s => s.GradingByUserId!.Value)
            .Distinct()
            .ToList();

        // Fetch their names in one query
        var userRepo = unitOfWork.GetOrCreateRepository<User, Guid>();
        var gradingUsers = gradingUserIds.Any()
            ? await userRepo.ListAsync(new UsersByIdsSpecification(gradingUserIds), ct)
            : new List<User>();

        var gradingUserNamesById = gradingUsers.ToDictionary(
            u => u.Id,
            u => $"{u.FirstName} {u.LastName}".Trim()
        );


        // 4. Build lookups for O(1) access
        var enrollmentsByLesson = enrollments.GroupBy(e => e.LessonId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var submissionsByAssignmentAndStudent = submissions
            .GroupBy(s => s.AssignmentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(s => s.StudentId)
            );

        var assignmentById = assignments.ToDictionary(a => a.Id);

        var now = DateTimeOffset.UtcNow;
        var allItems = new List<AssignmentSubmissionListItemDto>();

        // 5. Build result — no more DB calls inside loop
        foreach (var assignment in assignments)
        {
            enrollmentsByLesson.TryGetValue(assignment.LessonId, out var lessonEnrollments);
            if (lessonEnrollments is null) continue;

            submissionsByAssignmentAndStudent.TryGetValue(assignment.Id, out var submissionByStudent);
            submissionByStudent ??= new Dictionary<Guid, AssignmentSubmission>();

            foreach (var enrollment in lessonEnrollments)
            {
                var student = enrollment.Student!;
                submissionByStudent.TryGetValue(student.Id, out var submission);

                string status;
                int? score = null;
                DateTimeOffset? submittedAt = null;
                string? fileUrl = null;
                bool isBeingGraded = false;

                if (submission is null)
                {
                    status = "not_submitted";
                }
                else
                {
                    submittedAt = submission.SubmittedAt;
                    fileUrl = submission.FileUrl;
                    score = submission.Score;

                    var lockExpired = submission.GradingStartedAt.HasValue
                        && now - submission.GradingStartedAt.Value > GradingLockExpiry;

                    if (submission.IsBeingGraded && !lockExpired)
                    {
                        status = "grading";
                        isBeingGraded = true;
                    }
                    else if (submission.Score.HasValue)
                    {
                        status = "graded";
                    }
                    else
                    {
                        status = "pending";
                    }
                }

                allItems.Add(new AssignmentSubmissionListItemDto
                {
                    SubmissionId = submission?.Id ?? 0,
                    StudentId = student.Id,
                    StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                    AssignmentId = assignment.Id,
                    LessonTitle = assignment.Lesson?.Title ?? string.Empty,
                    FileUrl = fileUrl,
                    SubmittedAt = submittedAt,
                    Score = score,
                    MaxScore = assignment.Grade,
                    Status = status,
                    IsBeingGraded = isBeingGraded,
                    GradingByUserName = submission?.IsBeingGraded == true && submission.GradingByUserId.HasValue
                    ? gradingUserNamesById.GetValueOrDefault(submission.GradingByUserId.Value)
                    : null
                });
            }
        }

        // 6. Apply search filter (by student name)
        if (!string.IsNullOrWhiteSpace(request.Search))
            allItems = allItems
                .Where(i => i.StudentName
                    .Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        // 7. Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "all")
            allItems = allItems.Where(i => i.Status == request.Status).ToList();

        var totalCount = allItems.Count;

        // 8. Pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var pagedItems = allItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new AssignmentSubmissionsResponseDto
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
