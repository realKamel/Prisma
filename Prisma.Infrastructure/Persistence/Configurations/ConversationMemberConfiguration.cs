using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities.ChatAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal sealed class ConversationMemberConfiguration
    : IEntityTypeConfiguration<ConversationMember>
{
    public void Configure(EntityTypeBuilder<ConversationMember> builder)
    {
        builder.ToTable("ConversationMembers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<int>().IsRequired();

        // Inbox loading: all conversations for a user, and membership checks.
        builder.HasIndex(x => new { x.UserId, x.ConversationId }).IsUnique();
    }
}