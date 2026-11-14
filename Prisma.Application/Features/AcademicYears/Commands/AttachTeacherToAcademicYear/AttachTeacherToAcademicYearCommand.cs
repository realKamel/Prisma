using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.AcademicYears.Commands.AttachTeacherToAcademicYear;

public record AttachTeacherToAcademicYearCommand(int AcademicYearId, Guid? TeacherId)
    : ICommand<Result>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Teacher, AppRoles.Admin];
    public IReadOnlyList<string> RequiredPermissions { get; }
}
