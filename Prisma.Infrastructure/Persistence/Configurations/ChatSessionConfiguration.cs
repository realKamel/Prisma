using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SerializedSessionJson).HasColumnType("json");
        builder.Property(x => x.UserId).HasMaxLength(450); // matches AspNetUsers Id length
        builder.HasIndex(x => x.UserId);
    }
}