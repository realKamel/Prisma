using MediatR;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Payments.InitiatePayment;

public record InitiatePaymentCommand(
    int AmountCents,
    string Email,
    string FirstName,
    string LastName,
    PaymentMethod Method,
    Guid StudentId,
    int LessonId
) : IRequest<InitiatePaymentResult>;

public record InitiatePaymentResult(string ClientSecret, string PublicKey);