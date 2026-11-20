using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.DeattachLessonAssignment;

public sealed record DeattachLessonAssignmentCommand(Guid LessonId)
    : ICommand<Result>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
};
