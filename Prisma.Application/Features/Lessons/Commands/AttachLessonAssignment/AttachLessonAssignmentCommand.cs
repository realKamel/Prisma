using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Lessons.Commands.AttachLessonAssignment;

public sealed record AttachLessonAssignmentCommand(
    Guid LessonId,
    decimal TotalMarks,
    DateTimeOffset? AssignmentDueDate,
    FileMetadataDto File
) : ICommand<Result<AttachLessonAssignmentResponse>>, IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
};

public sealed record AttachLessonAssignmentResponse(string AssignmentUploadLink);
