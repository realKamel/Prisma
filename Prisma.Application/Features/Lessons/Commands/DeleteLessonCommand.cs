using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLesson;

public record DeleteLessonCommand(int LessonId) : IRequest<Result<string>>;