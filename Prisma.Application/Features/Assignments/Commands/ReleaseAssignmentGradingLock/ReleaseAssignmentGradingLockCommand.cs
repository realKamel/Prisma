using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.Assignments.Commands.ReleaseAssignmentGradingLock;

public record ReleaseAssignmentGradingLockCommand(int SubmissionId) : IRequest<Result>;

