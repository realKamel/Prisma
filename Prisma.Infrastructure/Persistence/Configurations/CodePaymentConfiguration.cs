using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.PaymentAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class CodePaymentConfiguration : IEntityTypeConfiguration<CodePayment>
{
    public void Configure(EntityTypeBuilder<CodePayment> builder)
    {
    }
}