using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.RedeemCodes.Commands.CreateCodeBatch;

public record CreateCodeBatchCommand(
    int AcademicYearId,
    int LessonId,
    int Count,
    string? Prefix
) : IRequest<Result<CreateCodeBatchResponse>>;

public class CreateCodeBatchResponse
{
    public int BatchId { get; set; }
    public List<string> Codes { get; set; } = new();
}