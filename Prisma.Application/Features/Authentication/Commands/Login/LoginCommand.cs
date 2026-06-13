using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string? Email, string? Phone, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, LoginCredentials Credentials);

public record LoginCredentials(Guid? Id, string? Email, string? FirstName, string? SecondName, string? Role);