using Prisma.Application.Common.DTOs.Ai;

namespace Prisma.Application.Abstractions.Services;

public interface ISummarizationServices
{
    Task<string> SummarizationAsync(LessonContentDto contentDto, CancellationToken cancellationToken = default);
}