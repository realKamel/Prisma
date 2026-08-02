using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Authentication.Commands.EmailVerification;

public record EmailVerificationRequestCommand(string Email) : IRequest<Result>;
public record ConfirmEmailCommand(string Email, string Token) : IRequest<Result>;