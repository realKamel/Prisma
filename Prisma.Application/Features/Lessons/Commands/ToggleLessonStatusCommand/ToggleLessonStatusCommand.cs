using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Commands.ToggleLessonStatus;

public record ToggleLessonStatusCommand(int Id) : IRequest<Result<string>>;