using Ardalis.Result;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.Interfaces;

namespace Prisma.Application.Features.Lessons.Commands.AddLesson;

public record AddLessonCommand(
    string Title,
    string? Description,
    decimal Price,
    string Currency,
    int? PrerequisiteLessonId,
    bool AssignmentEnabled,
    DateTimeOffset? AssignmentDueDate,
    int? AssignmentGrade,
    IReadOnlyList<int> AcademicYearIds,
    IReadOnlyList<string> Outcomes,
    long ValidityDurationInSeconds
) : IAuthorizableRequest<Result<AddLessonResponse>>
{
    public IReadOnlyList<string> RequiredRoles { get; } =
    [AppRoles.Admin, AppRoles.Teacher, AppRoles.Assistant];

    public IReadOnlyList<string> RequiredPermissions { get; } =
    [AppClaims.Policies.CanManageContent];
}

public record AddLessonResponse(Guid LessonId, string CoverUploadUrl, string? AssignmentUploadUrl);
