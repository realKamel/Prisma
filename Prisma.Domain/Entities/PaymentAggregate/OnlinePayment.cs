namespace Prisma.Domain.Entities.PaymentAggregate;

public class OnlinePayment : Payment
{
    public DateTimeOffset PaidAt { get; set; }

    public string? TransactionID { get; set; }

    public decimal Amount { get; set; }
}