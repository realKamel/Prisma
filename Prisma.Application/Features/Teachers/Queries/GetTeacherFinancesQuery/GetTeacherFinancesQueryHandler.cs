using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.DTOs;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherFinancesQuery;

internal sealed class GetTeacherFinancesQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetTeacherFinancesQuery, Result<List<RawTransactionDto>>>
{
    public async Task<Result<List<RawTransactionDto>>> Handle(
        GetTeacherFinancesQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized();
        }

        var paymentRepository = unitOfWork.GetOrCreateRepository<Payment, int>();

        var spec = new TeacherFinancesSpecification<Financesinfo>(userId.Value, p => new Financesinfo(
            Id: p.Id,
            Amount: p.Money,
            PaidAt: p.PaidAt,
            StudentFirstName: p.Student.FirstName,
            StudentLastName: p.Student.SecondName,
            LessonTitle: p.Lesson.Title
        ));


        var payments = await paymentRepository.ListAsync(spec, cancellationToken);

        var transactionsList = payments
            .Select(p => new RawTransactionDto(
                Id: p.Id.ToString(),
                StudentName: p.StudentFirstName != null
                    ? $"{p.StudentFirstName} {p.StudentLastName}".Trim()
                    : "طالب غير معروف",
                LessonTitle: p.LessonTitle ?? "درس غير معروف",
                Money: p.Amount.ToDto(),
                Date: p.PaidAt
            ))
            .ToList();

        return transactionsList;
    }
}

public record Financesinfo(
    int Id,
    Money Amount,
    DateTimeOffset? PaidAt,
    string? StudentFirstName,
    string? StudentLastName,
    string? LessonTitle
);