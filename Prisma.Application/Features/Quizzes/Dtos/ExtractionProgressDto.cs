namespace Prisma.Application.Features.Quizzes.Dtos;

public class ExtractionProgressDto
{
    public string State { get; set; } = "idle";
    public int Progress { get; set; }
    public string Phase { get; set; } = string.Empty;
    public string? Error { get; set; }
    public ExtractedQuestionDto? CurrentQuestion { get; set; }
    public List<ExtractedQuestionDto> CompletedQuestions { get; set; } = new();
}
