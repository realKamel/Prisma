using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Assignments.Commands.ReleaseAssignmentGradingLock;

public record ReleaseAssignmentGradingLockCommand(int SubmissionId) : IRequest<Result>;

