using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Assignments.Dtos;

namespace Prisma.Application.Features.Assignments.Queries.GetAssignmentSubmissionsList;

public record GetAssignmentSubmissionsListQuery(
    string? Search,       // search by student name
    int? LessonId,        // filter by lesson
    string? Status,       // filter: "not_submitted" | "pending" | "grading" | "graded"
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<AssignmentSubmissionsResponseDto>>;