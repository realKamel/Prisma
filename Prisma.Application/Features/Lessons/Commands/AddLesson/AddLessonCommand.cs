using Ardalis.Result;
using Prisma.Application.Abstractions.Authorization;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.Constants;

namespace Prisma.Application.Features.Lessons.Commands.AddLesson;

// public sealed record AddLessonCommand(
//     string Title,
//     string? Description,
//     decimal Price,
//     string Currency,
//     int? PrerequisiteLessonId,
//     bool AssignmentEnabled,
//     DateTimeOffset? AssignmentDueDate,
//     int? AssignmentGrade,
//     IReadOnlyList<int> AcademicYearIds,
//     IReadOnlyList<string> Outcomes,
//     long ValidityDurationInSeconds
// ) : ICommand<Result<AddLessonResponse>>, IAuthorizableRequest
// {
//     public IReadOnlyList<string> RequiredRoles { get; } =
//     [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];
//
//     public IReadOnlyList<string> RequiredPermissions { get; } =
//     [AppClaims.Policies.CanManageContent];
// }

// public sealed record AddLessonResponse(
//     Guid LessonId,
//     string CoverUploadUrl,
//     string? AssignmentUploadUrl
// );

public sealed record AddLessonCommand(
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    int? PrerequisiteLessonId,
    IReadOnlyList<int> AcademicYearIds,
    IReadOnlyList<string> Outcomes,
    long ValidityDurationInSeconds
) : ICommand<Result<AddLessonResponse>>, IAuthorizableRequest
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
}

public sealed record AddLessonResponse(Guid LessonId);
