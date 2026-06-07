using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken) : IRequest<Result<AuthResponse>>;

public record AuthResponse(string AcessToken, string RefershToken, DateTime RefreshTokenExpiry)
    : IRequest<Result<AuthResponse>>;