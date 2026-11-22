using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonMaterial;

public record DeleteLessonMaterialCommand(Guid PublicLessonId, int MaterialId)
    : ICommand<Result>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Teacher, AppRoles.Assistant, AppRoles.Admin];

    public IReadOnlyList<string> RequiredPermissions { get; }
}
