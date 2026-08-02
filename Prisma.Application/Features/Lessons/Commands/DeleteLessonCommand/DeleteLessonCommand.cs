using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;

public record DeleteLessonCommand(int LessonId) : IRequest<Result<string>>;