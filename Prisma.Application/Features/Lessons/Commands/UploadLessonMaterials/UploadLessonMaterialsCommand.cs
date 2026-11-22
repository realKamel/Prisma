using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.UploadLessonMaterials;

public sealed record UploadLessonMaterialsCommand(int LessonId, IList<IFormFile> Files)
    : ICommand<Result>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles =>
        [AppRoles.Teacher, AppRoles.Assistant, AppRoles.Admin];

    public IReadOnlyList<string> RequiredPermissions => [AppClaims.Policies.CanManageContent];
}
