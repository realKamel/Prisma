using MediatR;
using Prisma.Application.Features.TeacherStudents.Dtos;

namespace Prisma.Application.Features.TeacherStudents.Queries.GetTeacherLessonsForGrant;

public record GetTeacherLessonsForGrantQuery : IRequest<List<LessonForGrantDto>>;
