using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.RenewSectionUploadLink;

public sealed record RenewSectionUploadLinkCommand(Guid LessonId, Guid SectionId, Guid VideoId)
    : ICommand<Result<RenewSectionUploadLinkResponse>>,
        IAuthorizableRequest
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
