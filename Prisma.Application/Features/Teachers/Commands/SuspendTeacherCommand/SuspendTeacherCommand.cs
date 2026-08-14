using MediatR;
using Ardalis.Result;


namespace Prisma.Application.Features.Teachers.Commands.SuspendTeacherCommand;

public record SuspendTeacherCommand(Guid TeacherId, string Reason) : IRequest<Result<bool>>;
public record SuspendTeacherRequest(string Reason);
