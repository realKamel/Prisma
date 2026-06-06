using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Authentication.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FristName,
    string LastName) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(string accessToken, string refreshToken);