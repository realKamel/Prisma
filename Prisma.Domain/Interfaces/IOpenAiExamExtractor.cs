using System.Collections.Generic;
using System.Threading;
using Prisma.Domain.Entities.QuizAggregate;

namespace Prisma.Domain.Interfaces;

public interface IOpenAiExamExtractor
{
    IAsyncEnumerable<ExtractionProgress> ExtractQuestionsAsync(
        string pdfText, 
        CancellationToken cancellationToken = default);
}

public class ExtractionProgress
{
    public int ProgressPercent { get; set; }
    public string Phase { get; set; } = string.Empty;
    public ExtractedQuestion? CurrentQuestion { get; set; }
    public List<ExtractedQuestion> CompletedQuestions { get; set; } = new();
    public bool IsComplete { get; set; }
    public string? Error { get; set; }
}
