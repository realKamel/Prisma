using MediatR;
using Prisma.Application.Common.Responses;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Authentication.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
public record ConfirmCodeCommand(string Email, string Code) : IRequest<Result<string>>;
public record ResetPasswordCommand(string Email, string Token,  string NewPassword) : IRequest<Result>;