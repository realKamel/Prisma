using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Common.DTOs.Auth;

namespace Prisma.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string? Email, string? Phone, string Password)
    : ICommand<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, LoginCredentials Credentials);
