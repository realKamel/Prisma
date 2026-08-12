using MediatR;

namespace Prisma.Application.Features.Authentication.Commands.CleanUserAuthTokens;

public record CleanUserAuthTokensCommand(string? AccessToken) : IRequest;