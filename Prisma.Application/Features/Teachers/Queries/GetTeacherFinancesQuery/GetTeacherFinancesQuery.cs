using Ardalis.Result;
using MediatR;
using Prisma.Application.Common.DTOs;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherFinancesQuery;

public record GetTeacherFinancesQuery : IRequest<Result<List<RawTransactionDto>>>;

public record RawTransactionDto(
    string Id,
    string StudentName,
    string LessonTitle,
    MoneyDto Amount,
    DateTimeOffset? Date
);