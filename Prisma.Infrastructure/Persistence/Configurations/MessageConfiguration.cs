using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Content)
            .HasMaxLength(Conversation.MaxMessageLength)
            .IsRequired();

        // Cursor pagination and unread aggregation both filter by conversation,
        // then walk messages newest-first.
        builder.HasIndex(x => new { x.ConversationId, x.SentAt }).IsDescending(false, true);
    }
}