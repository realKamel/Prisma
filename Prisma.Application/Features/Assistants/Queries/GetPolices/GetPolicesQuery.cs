using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Assistants.Queries.GetPolices;

public record GetPolicesQuery() : IRequest<Result<string[]>>;