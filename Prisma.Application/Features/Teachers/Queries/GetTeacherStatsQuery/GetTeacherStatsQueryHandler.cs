using MediatR;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Entities.UserAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Teachers;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherStatsQuery;

public class GetTeacherStatsQueryHandler : IRequestHandler<GetTeacherStatsQuery, TeacherStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeacherStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TeacherStatsDto> Handle(
        GetTeacherStatsQuery request,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfLastMonth = startOfMonth.AddMonths(-1);

        var teacherRepo = _unitOfWork.GetOrCreateRepository<Teacher, Guid>();
        var studentRepo = _unitOfWork.GetOrCreateRepository<Student, Guid>();
        var paymentRepo = _unitOfWork.GetOrCreateRepository<Payment, int>();

        var totalTeachers = await teacherRepo.CountAsync(cancellationToken);

        var newTeachersThisMonth = await teacherRepo.CountAsync(
            new NewTeachersThisMonthSpecification(startOfMonth),
            cancellationToken
        );

        var activeTeachers = await teacherRepo.CountAsync(
            new ActiveTeachersSpecification(),
            cancellationToken
        );

        var totalStudents = await studentRepo.CountAsync(cancellationToken);

        var currentMonthPayments = await paymentRepo.ListAsync(
            new CurrentMonthPaymentsSpecification(startOfMonth),
            cancellationToken
        );
        var monthRevenue = currentMonthPayments.Sum(p => p.Amount);

        var lastMonthPayments = await paymentRepo.ListAsync(
            new LastMonthPaymentsSpecification(startOfMonth, startOfLastMonth),
            cancellationToken
        );
        var lastMonthRevenue = lastMonthPayments.Sum(p => p.Amount);

        double revenueChangePercent =
            lastMonthRevenue > 0
                ? (double)((monthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                : 0;

        return new TeacherStatsDto(
            totalTeachers,
            newTeachersThisMonth,
            activeTeachers,
            monthRevenue,
            Math.Round(revenueChangePercent, 1),
            totalStudents
        );
    }
}
