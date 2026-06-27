namespace Prisma.Application.Features.TeacherStudents.Dtos;

public record StudentListItemDto(
    Guid Id,
    string Name,
    string Grade,
    string LastActive,
    int Lessons,
    int AvgQuiz,
    bool Active,
    string? Phone,
    string? ParentPhone,
    List<string> LessonTitles);