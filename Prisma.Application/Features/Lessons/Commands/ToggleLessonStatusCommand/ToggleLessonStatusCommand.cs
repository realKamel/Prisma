using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Commands.ToggleLessonStatus;

public record ToggleLessonStatusCommand(int Id) : IRequest<Result<string>>;