using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
public record LoginResponse(string accessToken, string refreshToken);