using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;

public sealed record RedeemCodeCommand(string Code, Guid LessonId)
    : IRequest<Result<RedeemCodeResponse>>;

public sealed record RedeemCodeResponse
{
    public int EnrollmentId { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
