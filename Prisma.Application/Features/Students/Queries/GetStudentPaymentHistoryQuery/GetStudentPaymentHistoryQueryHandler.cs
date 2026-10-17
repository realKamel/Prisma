using Ardalis.Result;
using MediatR;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Features.Students.Queries.GetStudentPaymentHistory;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Enrollments;
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

        var spec = new EnrollmentWithProjectionSpec<EnrollmentInfo>(e=> e.StudentId == userId.Value,
            e => new EnrollmentInfo(
                Id: e.Id,
                EnrollmentMethod: e.EnrollmentMethod,
                PaymentStatus: e.Payment != null ? e.Payment.Status : PaymentStatus.Pending,
                ExpiresAt: e.ExpiresAt,
                PaymentProvider: e.Payment != null ? e.Payment.Provider : null,
                PaymentAmount: e.Payment != null ? e.Payment.Money!= null ? e.Payment.Money.Amount : 0 : 0,
                PaymentPaidAt: e.Payment != null ? e.Payment.PaidAt : null,
                PaymentCreatedAt: e.Payment != null ? e.Payment.CreatedAt : null,
                CreatedAt: e.CreatedAt,
                PaymentProviderRef: e.Payment != null ? e.Payment.ProviderRef : null,
                PaymentId: e.PaymentId,
                LessonTitle: e.Lesson!= null ? e.Lesson.Title : null,
                LessonPublicId: e.Lesson != null ? e.Lesson.PublicId : Guid.Empty
                ));
        var enrollments = await enrollmentRepository.ListAsync(spec, cancellationToken);

        var paymentList = new List<StudentPaymentDetailsDto>();

        foreach (var enrollment in enrollments)
        {
            bool isPaidViaCode = enrollment.EnrollmentMethod == EnrollmentMethod.RedeemCode;
            bool hasCompletedPayment = enrollment.PaymentStatus == PaymentStatus.Completed;
            bool isTeacherGrant = enrollment.EnrollmentMethod == EnrollmentMethod.TeacherGrant;

            if (!isPaidViaCode && !hasCompletedPayment && !isTeacherGrant)
                continue;

            bool isExpired = enrollment.ExpiresAt.HasValue && enrollment.ExpiresAt.Value < DateTimeOffset.UtcNow;
            string statusString = isExpired ? "expired" : "paid";

            string paymentMethod = (isPaidViaCode, enrollment.PaymentProvider?.ToUpperInvariant()) switch
            {
                (true, _) => "CODE",
                (false, "ONLINE") => "ONLINE",
                _ => "TEACHER_GRANT"
            };

            decimal amountPaid = isPaidViaCode
                ? 0
                : enrollment.PaymentAmount ?? 0;

            DateTimeOffset paymentDate = enrollment.PaymentPaidAt
                                         ?? enrollment.PaymentCreatedAt
                                         ?? enrollment.CreatedAt
                                         ?? DateTimeOffset.UtcNow;

            paymentList.Add(new StudentPaymentDetailsDto(
                Id: enrollment.PaymentProviderRef ?? enrollment.PaymentId?.ToString() ?? $"enr-{enrollment.Id}",
                LessonTitle: enrollment.LessonTitle ?? string.Empty,
                LessonId: enrollment.LessonPublicId,
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
    public sealed record EnrollmentInfo(
        int Id,
        EnrollmentMethod EnrollmentMethod,
        PaymentStatus PaymentStatus,
        DateTimeOffset? ExpiresAt,
        string? PaymentProvider,
        decimal? PaymentAmount,
        DateTimeOffset? PaymentPaidAt,
        DateTimeOffset? PaymentCreatedAt,
        DateTimeOffset? CreatedAt,
        string? PaymentProviderRef,
        int? PaymentId,
        string? LessonTitle,
        Guid LessonPublicId);
}