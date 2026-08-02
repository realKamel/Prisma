using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.TeacherStudents.Commands.RevokeLesson;

public record RevokeLessonCommand(
    Guid StudentId,
    int LessonId) : IRequest<Result>;
