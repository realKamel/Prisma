using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.TeacherStudents.Commands.GrantLesson;

public record GrantLessonCommand(
    Guid StudentId,
    int LessonId,
    int ValidityDays,
    string? Note) : IRequest<Result>;
