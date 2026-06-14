using MediatR;
using Prisma.Application.Common.Responses;

namespace Prisma.Application.Features.Authentication.Commands.Logout;

public record LogoutCommand(string? AccessToken) : IRequest;