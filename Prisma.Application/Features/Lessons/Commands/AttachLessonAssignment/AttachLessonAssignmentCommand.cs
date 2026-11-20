using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.AttachLessonAssignment;

public sealed record AttachLessonAssignmentCommand(
    Guid LessonId,
    decimal TotalMarks,
    DateTimeOffset? AssignmentDueDate,
    IFormFile File
) : ICommand<Result<AttachLessonAssignmentResponse>>, IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
};

public sealed record AttachLessonAssignmentResponse(string AssignmentUploadLink);
