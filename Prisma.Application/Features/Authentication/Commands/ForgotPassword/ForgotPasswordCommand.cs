using MediatR;
using Ardalis.Result;


namespace Prisma.Application.Features.Authentication.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
public record ConfirmCodeCommand(string Email, string Code) : IRequest<Result>;
public record ResetPasswordCommand(string Email, string NewPassword) : IRequest<Result>;