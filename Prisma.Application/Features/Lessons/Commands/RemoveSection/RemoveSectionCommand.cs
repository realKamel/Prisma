using Ardalis.Result;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.RemoveSection;

public record RemoveSectionCommand(Guid LessonId, Guid SectionId) : IAuthorizableRequest<Result>
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
}
