using Prisma.Application.Common.DTOs;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Students.Queries.GetPaginatedLessonsCatalog;

public record LessonCatalogDto
{
    public int Id { get; init; }
    public Guid PublicId { get; init; }
    public string? Title { get; init; }
    public MoneyDto? Money { get; init; }
    public EnrollmentStatus Status { get; init; }
    public string? PrerequisiteLabel { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public bool? IsExpired { get; init; }
    public string? TeacherName { get; init; }
    public string? Subject { get; init; }
    public TimeDurationDto Duration { get; init; }
    public string? ImageThumbnailUrl { get; init; }
}