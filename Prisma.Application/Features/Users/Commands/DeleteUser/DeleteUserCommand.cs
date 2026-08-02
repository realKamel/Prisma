using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;