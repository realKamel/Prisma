using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Teachers.Profile;

// Shapes mirror the Angular `my-profile.model.ts`.

public sealed record ContactLinksDto(
    string? Whatsapp,
    string? Facebook,
    string? Instagram,
    string? Youtube,
    string? Telegram,
    string? Tiktok,
    string? Website
);

public sealed record ProfileVisibilityDto(
    bool ShowYearsOfExperience,
    bool ShowContactLinks,
    bool ShowStudentsCount,
    bool ShowLessonsCount,
    bool ShowPopularLessons
);

/// <summary>A download link for the picture, or null when the teacher has none.</summary>
public sealed record ProfilePictureDto(string? Url);

public sealed record PopularLessonDto(Guid Id, string Title, int StudentsCount);

public sealed record ProfileStatsDto(
    int StudentsCount,
    int LessonsCount,
    IReadOnlyList<PopularLessonDto> PopularLessons
);

public sealed record MyProfileResponse(
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Subject,
    string? Bio,
    int? YearsOfExperience,
    ProfileColor Color,
    ContactLinksDto Links,
    ProfileVisibilityDto Visibility,
    ProfilePictureDto Picture,
    ProfileStatsDto Stats
);