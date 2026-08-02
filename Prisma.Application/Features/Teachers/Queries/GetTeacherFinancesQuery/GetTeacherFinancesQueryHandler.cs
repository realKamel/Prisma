using MediatR;
using Prisma.Application.Abstractions.Services;
using Ardalis.Result;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teacher;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherFinancesQuery;

public class GetTeacherFinancesQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetTeacherFinances.GetTeacherFinancesQuery, Result<List<RawTransactionDto>>>
{
    public async Task<Result<List<RawTransactionDto>>> Handle(GetTeacherFinances.GetTeacherFinancesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized("User is not authenticated.");
        }

        var paymentRepository = unitOfWork.GetOrCreateRepository<Payment, int>();

        var spec = new TeacherFinancesSpecification();
        var payments = await paymentRepository.ListAsync(spec, cancellationToken);

        var transactionsList = payments.Select(p => new RawTransactionDto(
            Id: p.Id.ToString(),
            StudentName: p.Student != null ? $"{p.Student.FirstName} {p.Student.LastName}".Trim() : "طالب غير معروف",
            LessonTitle: p.Lesson?.Title ?? "درس غير معروف",
            Amount: p.Amount,
            Date: p.PaidAt?.ToString("yyyy-MM-dd") ?? string.Empty
        )).ToList();

        return transactionsList;
    }
}