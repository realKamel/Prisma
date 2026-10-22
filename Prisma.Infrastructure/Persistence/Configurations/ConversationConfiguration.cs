using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Title).HasMaxLength(Conversation.MaxTitleLength);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasMany(x => x.Members)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Members).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.LastMessageAt);
    }
}