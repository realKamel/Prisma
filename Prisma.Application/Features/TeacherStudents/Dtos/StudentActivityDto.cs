namespace Prisma.Application.Features.TeacherStudents.Dtos;

public record StudentActivityDto(
    string Message,
    DateTimeOffset Time,
    string DotColor);