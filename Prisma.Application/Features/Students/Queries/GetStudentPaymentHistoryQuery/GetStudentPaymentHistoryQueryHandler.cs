using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Students;

namespace Prisma.Application.Features.Students.Queries.GetStudentPaymentHistoryQuery;

internal sealed class GetStudentPaymentHistoryQueryHandler(
    IUnitOfWork _unitOfWork,
    ICurrentUserService _currentUserService
) : IRequestHandler<GetStudentPaymentHistory.GetStudentPaymentHistoryQuery, Result<StudentPaymentHistoryResponseDto>>
{
    public async Task<Result<StudentPaymentHistoryResponseDto>> Handle(
        GetStudentPaymentHistory.GetStudentPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result.Unauthorized();

        var enrollmentRepository = _unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var spec = new StudentEnrollmentsWithPaymentsSpec(userId.Value);
        var enrollments = await enrollmentRepository.ListAsync(spec, cancellationToken);

        var paymentList = new List<StudentPaymentDetailsDto>();

        foreach (var enrollment in enrollments)
        {
            bool isPaidViaCode = enrollment.EnrollmentMethod == EnrollmentMethod.RedeemCode;
            bool hasCompletedPayment = enrollment.Payment?.Status == PaymentStatus.Completed;
            bool isTeacherGrant = enrollment.EnrollmentMethod == EnrollmentMethod.TeacherGrant;

            if (!isPaidViaCode && !hasCompletedPayment && !isTeacherGrant)
                continue;

            bool isExpired = enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTimeOffset.UtcNow;
            string statusString = isExpired ? "expired" : "paid";

            string paymentMethod = (isPaidViaCode, enrollment.Payment?.Provider?.ToLowerInvariant()) switch
            {
                (true, _) => "code",
                (false, "online") => "online",
                _ => "teacher grant"
            };

            decimal amountPaid = isPaidViaCode
                ? 0
                : enrollment.Payment?.Money.Amount ?? 0;

            DateTimeOffset paymentDate = enrollment.Payment?.PaidAt
                                         ?? enrollment.Payment?.CreatedAt
                                         ?? enrollment.CreatedAt
                                         ?? DateTimeOffset.UtcNow;

            paymentList.Add(new StudentPaymentDetailsDto(
                Id: enrollment.Payment?.ProviderRef ?? enrollment.PaymentId?.ToString() ?? $"enr-{enrollment.Id}",
                LessonTitle: enrollment.Lesson?.Title ?? string.Empty,
                LessonId: enrollment.LessonId ?? 0,
                PosterVariant: "energy",
                PaymentDate: paymentDate,
                Amount: amountPaid,
                Method: paymentMethod,
                Status: statusString
            ));
        }

        decimal totalAmount = paymentList.Sum(p => p.Amount);
        int lessonsPurchased = paymentList.Count;
        int activeLessons = paymentList.Count(p => p.Status == "paid");
        int expiredLessons = paymentList.Count(p => p.Status == "expired");

        var statsDto = new PaymentStatsDto(
            TotalAmount: totalAmount,
            LessonsPurchased: lessonsPurchased,
            ActiveLessons: activeLessons,
            ExpiredLessons: expiredLessons
        );

        return Result<StudentPaymentHistoryResponseDto>.Success(
            new StudentPaymentHistoryResponseDto(statsDto, paymentList)
        );
    }
}