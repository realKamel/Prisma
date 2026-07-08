using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.AdminDashboard;

namespace Prisma.Application.Features.AdminDashboard.Queries.GetAdminActivities;

public class GetAdminActivitiesQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAdminActivitiesQuery, Result<List<AdminActivityDto>>>
{
    public async Task<Result<List<AdminActivityDto>>> Handle(GetAdminActivitiesQuery request, CancellationToken cancellationToken)
    {
        var enrollmentRepo = _unitOfWork.GetOrCreateRepository<Enrollment, int>();
        var paymentRepo = _unitOfWork.GetOrCreateRepository<Payment, int>();

        var rawActivities = new List<(DateTimeOffset TimeStamp, AdminActivityDto Activity)>();

        var enrollSpec = new AdminLatestEnrollmentsSpec();
        var latestEnrollments = await enrollmentRepo.ListAsync(enrollSpec, cancellationToken);

        foreach (var enroll in latestEnrollments)
        {
            if (enroll == null) continue;

            DateTimeOffset enrollDate = enroll.CreatedAt ?? DateTimeOffset.UtcNow;

            var activityDto = new AdminActivityDto(
                Id: $"act-enr-{enroll.Id}",
                Type: "enroll",
                EntityId: enroll.StudentId?.ToString() ?? string.Empty,
                Details: enroll.Lesson?.Title ?? string.Empty,
                MetaInfo: enroll.EnrollmentMethod.ToString(), 
                ActivityDate: enrollDate
            );

            rawActivities.Add((enrollDate, activityDto));
        }

        var paySpec = new AdminSuccessfulPaymentsSpec();
        var successfulPayments = await paymentRepo.ListAsync(paySpec, cancellationToken);
        var latestPayments = successfulPayments.OrderByDescending(p => p.PaidAt).Take(5);

        foreach (var pay in latestPayments)
        {
            if (pay == null) continue;

            DateTimeOffset timeStamp = pay.PaidAt ?? pay.CreatedAt ?? DateTimeOffset.UtcNow;
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