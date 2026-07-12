namespace Prisma.Application.Features.RedeemCodes.Dtos;

public class CodeLessonOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
}