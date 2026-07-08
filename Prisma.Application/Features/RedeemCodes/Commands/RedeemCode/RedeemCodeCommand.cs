using MediatR;
using Prisma.Application.Common.Responses.Generic;

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