using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.AddLessonMaterial;

public sealed record AddLessonMaterialCommand(
    Guid LessonId,
    string FileName,
    string ContentType,
    long Size
) : ICommand<Result<AddLessonMaterialCommandResponse>>, IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles =>
        [AppRoles.Teacher, AppRoles.Assistant, AppRoles.Admin];

    public IReadOnlyList<string> RequiredPermissions => [AppClaims.Policies.CanManageContent];
}

public sealed record AddLessonMaterialCommandResponse(int MaterialId, string PresignedUploadUrl);
