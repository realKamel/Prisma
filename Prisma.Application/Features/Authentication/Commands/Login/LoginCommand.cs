using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string? Email, string? Phone, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, User user);