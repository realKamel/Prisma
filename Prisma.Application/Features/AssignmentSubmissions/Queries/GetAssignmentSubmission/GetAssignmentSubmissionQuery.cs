using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.AssignmentSubmissions.Queries.GetAssignmentSubmission;

public sealed record GetAssignmentSubmissionQuery(int LessonId) : IRequest<Result<AssignmentSubmissionDto?>>;

public sealed class AssignmentSubmissionDto
{
    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTimeOffset SubmittedAt { get; set; }
    public int? Score { get; set; }
    public string? Notes { get; set; }
}