using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Application.Features.Teachers.Profile;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Teachers.Commands.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(
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
    string? AnnouncementText,
    DateTimeOffset? AnnouncementEndsAt,
    bool RemovePicture
) : ICommand<Result<bool>>, IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Teacher];

    public IReadOnlyList<string> RequiredPermissions { get; } = [];
}