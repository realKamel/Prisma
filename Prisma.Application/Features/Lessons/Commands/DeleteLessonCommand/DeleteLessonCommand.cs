using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;

public record DeleteLessonCommand(int LessonId) : IRequest<Result>;
