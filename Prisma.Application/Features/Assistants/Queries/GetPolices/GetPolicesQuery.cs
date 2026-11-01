using Ardalis.Result;
using MediatR;

namespace Prisma.Application.Features.Assistants.Queries.GetPolices;

public sealed record GetPolicesQuery() : IRequest<Result<string[]>>;
