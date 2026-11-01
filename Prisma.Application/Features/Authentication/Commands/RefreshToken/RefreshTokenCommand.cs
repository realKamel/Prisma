using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string? AccessToken, string? RefreshToken)
    : IRequest<Result<AuthResponse>>;

public sealed record AuthResponse(string AccessToken, string RefreshToken)
    : IRequest<Result<AuthResponse>>;
