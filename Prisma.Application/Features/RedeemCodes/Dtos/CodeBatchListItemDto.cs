namespace Prisma.Application.Features.RedeemCodes.Dtos;

public class CodeBatchListItemDto
{
    public int Id { get; set; }
    public int AcademicYearId { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public string Lesson { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public int TotalCodes { get; set; }
    public int UsedCodes { get; set; }
}