namespace Prisma.API.Features.TeacherStudents.Requests;

public sealed record GrantLessonRequest(
    Guid StudentId,
    int LessonId,
    int ValidityDays,
    string? Note
);
