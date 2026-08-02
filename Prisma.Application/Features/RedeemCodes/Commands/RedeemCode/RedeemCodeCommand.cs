using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.RedeemCodes.Commands.RedeemCode;

public record RedeemCodeCommand(
    string Code,
    int LessonId
) : IRequest<Result<RedeemCodeResponse>>;

public class RedeemCodeResponse
{
    public int EnrollmentId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}