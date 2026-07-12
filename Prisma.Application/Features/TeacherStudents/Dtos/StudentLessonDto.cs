namespace Prisma.Application.Features.TeacherStudents.Dtos;

public record StudentLessonDto(
    int Id,
    string Title,
    string Method,
    string GrantedBy,
    string Status,
    int Progress,
    string StatusColor,
    string ProgressColor);
