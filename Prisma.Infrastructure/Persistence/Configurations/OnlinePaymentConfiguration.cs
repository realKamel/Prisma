using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class OnlinePaymentConfiguration : IEntityTypeConfiguration<OnlinePayment>
{
    public void Configure(EntityTypeBuilder<OnlinePayment> builder)
    {
        builder.Property(p => p.Amount)
            .HasPrecision(12, 2);
    }
}