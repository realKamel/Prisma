using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.TeacherStudents.Commands.RevokeLesson;

public record RevokeLessonCommand(
    Guid StudentId,
    int LessonId) : IRequest<Result>;
