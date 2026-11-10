using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.CreateLessonSection;

public sealed record CreateLessonSectionCommand(
    Guid LessonId,
    string Title,
    long VideoDurationInSeconds = 0,
    bool IsPreviewable = false
) : ICommand<Result<CreateSectionAndAttachVideoUploadLinkResponse>>, IAuthorizableRequest
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
