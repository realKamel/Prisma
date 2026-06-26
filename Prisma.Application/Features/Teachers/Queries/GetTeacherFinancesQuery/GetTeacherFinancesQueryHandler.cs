using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Application.Features.Teachers.Queries.GetTeacherLessons;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherFinances;

public class GetTeacherFinancesQueryHandler(
    IUnitOfWork _unitOfWork, ICurrentUserService _currentUserService
) : IRequestHandler<GetTeacherFinancesQuery, Result<List<RawTransactionDto>>>
{
    public async Task<Result<List<RawTransactionDto>>> Handle(GetTeacherFinancesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            if (userId is null)
                throw new UnauthorizedAccessException("User is not authenticated.");

        var paymentRepository = _unitOfWork.GetOrCreateRepository<Payment, int>();

        var spec = new TeacherFinancesSpecification();
        var payments = await paymentRepository.ListAsync(spec, cancellationToken);

        var transactionsList = payments.Select(p => new RawTransactionDto(
            Id: p.Id.ToString(),
            StudentName: p.Student != null ? $"{p.Student.FirstName} {p.Student.LastName}".Trim() : "طالب غير معروف",
            LessonTitle: p.Lesson?.Title ?? "درس غير معروف",
            Amount: p.Amount,
            Date: p.PaidAt?.ToString("yyyy-MM-dd") ?? string.Empty
        )).ToList();

        return Result<List<RawTransactionDto>>.Success(transactionsList);
    }
}