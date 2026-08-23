using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherFinancesQuery;

public class GetTeacherFinancesQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetTeacherFinances.GetTeacherFinancesQuery, Result<List<RawTransactionDto>>>
{
    public async Task<Result<List<RawTransactionDto>>> Handle(
        GetTeacherFinances.GetTeacherFinancesQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return Result.Unauthorized("User is not authenticated.");
        }

        var paymentRepository = unitOfWork.GetOrCreateRepository<Payment, int>();
        
        var spec = new TeacherFinancesSpecification<Financesinfo>(userId.Value, p => new Financesinfo(
            Id: p.Id,
            Amount: p.Amount,
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
                Amount: p.Amount,
                Date: p.PaidAt?.ToString("yyyy-MM-dd") ?? string.Empty
            ))
            .ToList();

        return transactionsList;
    }
}

public record Financesinfo(
    int Id,
    decimal Amount,
    DateTimeOffset? PaidAt,
    string? StudentFirstName,
    string? StudentLastName,
    string? LessonTitle
);

