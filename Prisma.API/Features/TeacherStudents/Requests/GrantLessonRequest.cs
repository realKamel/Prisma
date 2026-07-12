namespace Prisma.API.Features.TeacherStudents.Requests;

public record GrantLessonRequest(
    Guid StudentId,
    int LessonId,
    int ValidityDays,
    string? Note);
