using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Lessons.Commands.ToggleLessonStatusCommand;

public record ToggleLessonStatusCommand(int Id) : IRequest<Result>;
