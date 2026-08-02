using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Assistants.Queries.GetPolices;

public record GetPolicesQuery() : IRequest<Result<string[]>>;