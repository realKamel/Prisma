namespace Prisma.Application.Features.Students.Queries.GetLessonsCatalog;

public record LessonCatalogDto
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public decimal Price { get; init; }
    public string Status { get; init; }
    public string? PrerequisiteLabel { get; init; }
    public string? ExpiredDate { get; init; }
    public string? TeacherName { get; init; }
    public string? Subject { get; init; }
    public int DurationMinutes { get; init; }
    public string? ImageThumbnailUrl { get; init; }
    public string Currency { get; init; } = "EGP";
}