using Prisma.Domain.Enums;

namespace Prisma.Domain.ReadModels.Lessons;

public record LessonCatalogReadModel(
    int Id,
    string? Title,
    decimal Price,
    LessonCatalogStatus Status,
    string? PrerequisiteLabel,
    string? ExpiredDate,
    string? TeacherName,
    string? Subject,
    int DurationHours,
    string? ImageThumbnailUrl,
    string Currency = "جنيه"
);
