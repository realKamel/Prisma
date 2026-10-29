using Ardalis.Result;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonSection;

public sealed record CreateLessonSectionCommand(
    Guid LessonId,
    string Title,
    long VideoDurationInSeconds = 0,
    bool IsPreviewable = false
) : IAuthorizableRequest<Result<CreateSectionAndAttachVideoUploadLinkResponse>>
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Teacher, AppRoles.Assistant, AppRoles.Admin];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
}

public sealed record CreateSectionAndAttachVideoUploadLinkResponse(
    Guid SectionId,
    Guid VideoId,
    // int SectionSortOrder,
    long LibraryId,
    long ExpirationTime,
    string Signature
);
