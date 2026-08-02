using MediatR;
using Prisma.Application.Common.DTOs.Auth;
using Ardalis.Result;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string? Email, string? Phone, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, LoginCredentials Credentials);