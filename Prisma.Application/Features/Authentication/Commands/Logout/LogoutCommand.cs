using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.Authentication.Commands.Logout;

public class LogoutCommand : IRequest<Result>;