using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetStudentLessons;

public record GetStudentLessonsQuery(Guid StudentId) : IRequest<List<StudentLessonDto>>;
