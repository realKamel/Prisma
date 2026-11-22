using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Lessons.Commands.AddLessonCover;

public sealed record AddLessonCoverCommand(Guid PublicLessonId, FileDto FileDto, Stream Content)
    : ICommand<Result>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
}
