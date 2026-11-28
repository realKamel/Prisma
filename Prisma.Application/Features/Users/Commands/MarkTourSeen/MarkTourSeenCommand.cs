using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Users.Commands.MarkTourSeen;

public sealed record MarkTourSeenCommand(string TourKey) : ICommand<Result>;