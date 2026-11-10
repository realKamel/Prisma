using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.AssignmentSubmissions.Commands.SubmitAssignment;

public sealed record SubmitAssignmentCommand(Guid LessonId, IFormFile File)
    : ICommand<Result>,
        IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } = [AppRoles.Student];
    public IReadOnlyList<string> RequiredPermissions { get; }
}
