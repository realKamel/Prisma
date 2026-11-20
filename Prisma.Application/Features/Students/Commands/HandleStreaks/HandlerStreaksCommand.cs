

using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Students.Commands.HandleStreaks;

public record HandlerStreaksCommand() : ICommand<Result>;