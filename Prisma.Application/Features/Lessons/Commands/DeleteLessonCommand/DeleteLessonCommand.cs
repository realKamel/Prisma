using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Lessons.Commands.DeleteLessonCommand;

public sealed record DeleteLessonCommand(int LessonId) : ICommand<Result>;
