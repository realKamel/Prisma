using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;

namespace Prisma.Application.Features.Payments.InitiatePayment;

public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    private readonly IServiceProvider _sp;
    private readonly IUnitOfWork _unitOfWork;

    public InitiatePaymentCommandHandler(IServiceProvider sp, IUnitOfWork unitOfWork)
    {
        _sp = sp;
        _unitOfWork = unitOfWork;
    }

    public async Task<InitiatePaymentResult> Handle(InitiatePaymentCommand request, CancellationToken ct)
    {
        var key = request.Method.ToString().ToLower();
        var paymentService = _sp.GetRequiredKeyedService<IPaymentService>(key);

        var (clientSecret, publicKey, paymobOrderId) = await paymentService.GetPaymentKeyAsync(
            request.AmountCents, request.Email, request.FirstName, request.LastName
        );

        var paymentRepo = _unitOfWork.GetOrCreateRepository<Payment, int>();

        var payment = new Payment
        {
            Provider = "Paymob",
            ProviderRef = paymobOrderId,
            Amount = request.AmountCents / 100m,
            Currency = "EGP",
            Status = PaymentStatus.Pending,
            StudentId = request.StudentId,
            LessonId = request.LessonId
        };

        paymentRepo.Add(payment);
        await _unitOfWork.SaveChangesAsync(ct);

        return new InitiatePaymentResult(clientSecret, publicKey);
    }
}