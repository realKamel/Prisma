using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Assignments.Commands.GradeAssignmentSubmission;

public record GradeAssignmentSubmissionCommand(
    int SubmissionId,
    int Score,
    string? Note
) : IRequest<Result>;