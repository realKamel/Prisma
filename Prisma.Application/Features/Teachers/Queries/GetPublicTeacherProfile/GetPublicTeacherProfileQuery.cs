using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Students.Queries.GetTeacherCatalog;
using Prisma.Application.Features.Teachers.Profile;
using Prisma.Domain.Enums;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Application.Features.Teachers.Queries.GetPublicTeacherProfile;

public sealed record GetPublicTeacherProfileQuery(Guid Id)
    : IQuery<Result<PublicTeacherProfileResponse>>;

/// <summary>
/// What students (and anonymous visitors) see on a teacher's public page.
/// Anything the teacher turned off in their visibility settings comes back as null,
/// so a hidden value never leaves the server.
/// </summary>
public record PublicTeacherProfileResponse(
    Guid Id,
    string FirstName,
    string SecondName,
    string Subject,
    string? Bio,
    string? ImageUrl,
    // Gradient preset for the hero (camelCase string via the global enum setting).
    ProfileColor Color,
    // Active lessons. Null when the teacher hides it.
    int? LessonsCount,
    // Active students (TeacherStudents rows). Null when the teacher hides it.
    int? TotalStudents,
    // Years of teaching experience. Null when not set or hidden.
    int? YearsOfExperience,
    // Null when hidden or when the teacher has no links at all.
    ContactLinksDto? Links,
    IReadOnlyList<AcademicYearDto> AcademicYears,
    // The teacher's latest active lessons (max 3). Empty when the teacher hides the lessons section
    // or when the visitor isn't signed in.
    IReadOnlyList<ProfileLessonDto> Lessons,
    // The teacher's pinned message. Null when there is none or it has ended.
    AnnouncementDto? Announcement,
    // True for a signed-in visitor who isn't the teacher: the page shows its chat buttons.
    bool CanMessage,
    // True for any signed-in visitor (the teacher included). Anonymous visitors get a sign-in step.
    bool IsSignedIn,
    bool Featured = false
);

public record AnnouncementDto(string Text, DateTimeOffset? EndsAt);

/// <summary>
/// One lesson card on the public profile. No enrollment status: the page is public and cached
/// for every visitor. Teacher name and subject come from the profile itself.
/// </summary>
public record ProfileLessonDto(
    Guid PublicId,
    string Title,
    TimeDuration Duration,
    Money Money,
    // Download link built by the handler from the stored object key. Null when there is no cover.
    string? ImageThumbnailUrl,
    int StudentsCount,
    int SectionsCount
);