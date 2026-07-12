namespace Prisma.Application.Features.TeacherStudents.Dtos;

public record StudentListItemDto(
    Guid Id,
    string Name,           // combined — used in the list table
    string FirstName,
    string SecondName,
    string ThirdName,
    string LastName,
    string Email,
    string Grade,          // title string — used in the list table
    int GradeId,           // numeric id — used to pre-select edit dropdown
    string LastActive,
    int Lessons,
    int AvgQuiz,
    bool Active,
    string? Phone,
    string? ParentPhone,
    List<string> LessonTitles);