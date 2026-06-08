namespace Prisma.Domain.Entities.PaymentAggregate;

public class CodePayment : Payment
{
    public string? CodeValue { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public bool IsUsed { get; set; }

    public Guid UsedBy { get; set; }
}