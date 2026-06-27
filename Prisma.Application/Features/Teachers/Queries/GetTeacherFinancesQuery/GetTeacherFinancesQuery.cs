using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;

public record GetTeacherFinancesQuery : IRequest<Result<List<RawTransactionDto>>>;

public record RawTransactionDto(
    string Id,
    string StudentName,
    string LessonTitle,
    decimal Amount,
    string Date
);