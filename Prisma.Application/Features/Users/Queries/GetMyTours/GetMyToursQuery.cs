using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;

namespace Prisma.Application.Features.Users.Queries.GetMyTours;

public sealed record GetMyToursQuery : IQuery<Result<IReadOnlyCollection<string>>>;