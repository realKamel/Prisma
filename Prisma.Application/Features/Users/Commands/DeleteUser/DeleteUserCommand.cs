using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;