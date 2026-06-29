using MediatR;

namespace Prisma.Application.Features.Payments.HandleCallback;
public record HandlePaymentCallbackCommand(
    string OrderId,
    bool Success,
    string TransactionId
) : IRequest;