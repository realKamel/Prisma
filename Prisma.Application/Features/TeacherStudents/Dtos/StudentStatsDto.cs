namespace Prisma.Application.Features.TeacherStudents.Dtos;

public record StudentStatsDto(
    int Lessons,
    int AvgQuiz,
    int Hours,
    int Pending);
