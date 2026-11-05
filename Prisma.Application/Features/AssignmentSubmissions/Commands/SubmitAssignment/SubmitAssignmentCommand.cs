using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.AssignmentSubmissions.Commands.SubmitAssignment;

public sealed record SubmitAssignmentCommand(Guid LessonId, IFormFile File)
    : IAuthorizableRequest<Result>
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Student];
    public IReadOnlyList<string> RequiredPermissions { get; }
}
