using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Teachers.Commands.ActivateTeacherCommand;

public record ActivateTeacherCommand(Guid TeacherId) : IRequest<Result<bool>>;