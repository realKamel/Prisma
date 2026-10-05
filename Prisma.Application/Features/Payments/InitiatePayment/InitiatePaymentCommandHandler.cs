using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Domain.Entities.PaymentAggregate;
using Prisma.Domain.Enums;
using Prisma.Domain.Interfaces;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Features.Payments.InitiatePayment;

internal sealed class InitiatePaymentCommandHandler(IServiceProvider sp, IUnitOfWork unitOfWork)
    : IRequestHandler<InitiatePaymentCommand, InitiatePaymentResult>
{
    public async Task<InitiatePaymentResult> Handle(InitiatePaymentCommand request, CancellationToken ct)
    {
        var key = request.Method.ToString().ToLower();
        var paymentService = sp.GetRequiredKeyedService<IPaymentService>(key);

        var (clientSecret, publicKey, paymobOrderId) = await paymentService.GetPaymentKeyAsync(
            request.AmountCents, request.Email, request.FirstName, request.LastName
        );

        var paymentRepo = unitOfWork.GetOrCreateRepository<Payment, int>();

        var amountResult = Money.Create(request.AmountCents / 100m, request.Currency);

        var payment = new Payment
        {
            Provider = "Paymob",
            ProviderRef = paymobOrderId,
            Money = amountResult,
            Status = PaymentStatus.Pending,
            StudentId = request.StudentId,
            LessonId = request.LessonId
        };

        await paymentRepo.AddAsync(payment, ct);

        return new InitiatePaymentResult(clientSecret, publicKey);
    }
}