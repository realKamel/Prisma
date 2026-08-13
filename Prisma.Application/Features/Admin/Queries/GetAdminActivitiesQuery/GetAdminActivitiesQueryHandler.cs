using MediatR;
using Ardalis.Result;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Admin;

namespace Prisma.Application.Features.Admin.Queries.GetAdminActivitiesQuery;

public class GetAdminActivitiesQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAdminActivitiesQuery, Result<List<AdminActivityDto>>>
{
    public async Task<Result<List<AdminActivityDto>>> Handle(GetAdminActivitiesQuery request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = _unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var paymentRepo = _unitOfWork.GetOrCreateRepository<Payment, int>();

        var rawActivities = new List<(DateTimeOffset TimeStamp, AdminActivityDto Activity)>();

        var latestEnrollments = await enrollmentRepo.ListAsync(new AdminLatestEnrollmentsSpec(), cancellationToken);

        foreach (var enroll in latestEnrollments)
        {
            if (enroll == null) continue;

            DateTimeOffset enrollDate = enroll.CreatedAt ?? DateTimeOffset.UtcNow;

            var activityDto = new AdminActivityDto(
                Id: $"act-enr-{enroll.Id}",
                Type: "enroll",
                EntityId: enroll.StudentId?.ToString() ?? string.Empty,
                Details: enroll.LessonTitle ?? string.Empty,
                MetaInfo: enroll.EnrollmentMethod.ToString(),
                ActivityDate: enrollDate
            );

            rawActivities.Add((enrollDate, activityDto));
        }

        var latestPayments = await paymentRepo.ListAsync(new AdminSuccessfulPaymentsSpec(), cancellationToken);

        foreach (var pay in latestPayments)
        {
            var timeStamp = pay.PaidAt ?? pay.CreatedAt ?? DateTimeOffset.UtcNow;

            var activityDto = new AdminActivityDto(
                Id: $"act-pay-{pay.Id}",
                Type: "payment",
                EntityId: pay.StudentId.ToString(),
                Details: $"{pay.Amount} {pay.Currency}",
                MetaInfo: $"{pay.Provider} - {pay.ProviderRef}",
                ActivityDate: timeStamp
            );

            rawActivities.Add((timeStamp, activityDto));
        }

        var finalActivities = rawActivities
            .OrderByDescending(x => x.TimeStamp)
            .Select(x => x.Activity)
            .Take(6)
            .ToList();

        return Result<List<AdminActivityDto>>.Success(finalActivities);
    }
}