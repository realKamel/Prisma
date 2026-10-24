namespace Prisma.Application.Features.AcademicYears.Dtos;

public sealed record AcademicYearOptionDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
