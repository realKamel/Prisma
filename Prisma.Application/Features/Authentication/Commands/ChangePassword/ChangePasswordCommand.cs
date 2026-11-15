using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Authentication.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword)
    : ICommand<Result<bool>>;
