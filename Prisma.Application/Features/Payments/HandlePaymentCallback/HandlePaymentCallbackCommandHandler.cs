using MediatR;
using Prisma.Domain.Entities.EnrollmentAggregate;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Specifications.Payments;

namespace Prisma.Application.Features.Payments.HandleCallback;

public class HandlePaymentCallbackCommandHandler : IRequestHandler<HandlePaymentCallbackCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public HandlePaymentCallbackCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(HandlePaymentCallbackCommand request, CancellationToken ct)
    {
        if (!request.Success) return;

        var paymentRepo = _unitOfWork.GetOrCreateRepository<Payment, int>();
        var enrollmentRepo = _unitOfWork.GetOrCreateRepository<Enrollment, int>();

        var payment = await paymentRepo.FirstOrDefaultAsync(new PaymentByProviderRefSpec(request.OrderId), ct);
        if (payment is null) return;

        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTimeOffset.UtcNow;

        var enrollment = new Enrollment
        {
            StudentId = payment.StudentId,
            LessonId = payment.LessonId,
            PaymentId = payment.Id,
            EnrollmentMethod = EnrollmentMethod.OnlinePayment,
            Status = EnrollmentStatus.Active,
        };

        enrollmentRepo.Add(enrollment);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
