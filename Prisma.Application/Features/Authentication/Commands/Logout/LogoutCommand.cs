using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Authentication.Commands.Logout;

public record LogoutCommand(string? AccessToken) : IRequest<Result>;