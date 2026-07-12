using Prisma.Domain.Common;
using Prisma.Domain.Enums;

namespace Prisma.Domain.Entities.QuizAggregate;

public class ExtractionJob : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public ExtractionStatus Status { get; set; } = ExtractionStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<ExtractedQuestion> Questions { get; set; } = new();
}
