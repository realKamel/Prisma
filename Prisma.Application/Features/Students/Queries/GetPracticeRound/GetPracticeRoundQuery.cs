using Ardalis.Result;
using Prisma.Application.Abstractions.Messaging;
using Prisma.Application.Features.Students.Common;

namespace Prisma.Application.Features.Students.Queries.GetPracticeRound;

public sealed record GetPracticeRoundQuery() : IQuery<Result<PracticeRoundDto>>;
