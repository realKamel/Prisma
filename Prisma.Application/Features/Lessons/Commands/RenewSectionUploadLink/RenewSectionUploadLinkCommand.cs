using Ardalis.Result;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.RenewSectionUploadLink;

public sealed record RenewSectionUploadLinkCommand(Guid LessonId, Guid SectionId, Guid VideoId)
    : IAuthorizableRequest<Result<RenewSectionUploadLinkResponse>>
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Teacher, AppRoles.Assistant, AppRoles.Admin];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
}

public sealed record RenewSectionUploadLinkResponse(
    Guid SectionId,
    Guid VideoId,
    long LibraryId,
    long ExpirationTime,
    string Signature
);
