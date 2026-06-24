namespace Prisma.Application.Features.Quizzes.Commands.ExtractQuestionsFromPdf;

public class ExtractionJobDto
{
    public int JobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
