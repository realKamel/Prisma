using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Identity;
using Prisma.Application.Abstractions.Services;

namespace Prisma.Application.Features.Quizzes.Commands.ReportSecurityEvent;

public class ReportSecurityEventCommandHandler(
    IAttemptSecurityCounter counter,
    ICurrentUserService currentUser
) : IRequestHandler<ReportSecurityEventCommand, Result>
{
    public async Task<Result> Handle(ReportSecurityEventCommand request, CancellationToken ct)
    {
        var studentId = currentUser.UserId!.Value;

        // false = attempt missing, not the student's, or already finalized: nothing to record
        await counter.IncrementAsync(request.AttemptId, studentId, request.EventType, ct);

        return Result.Success();
    }
}
