namespace Prisma.Domain.Entities.UserAggregate;

public sealed class ChatHistoryMessageEntity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int Sequence { get; set; }
    public string Role { get; set; } = default!; // denormalized for querying/debugging
    public string MessageJson { get; set; } = default!; // full ChatMessage, serialized
    public DateTimeOffset CreatedAtUtc { get; set; }
}