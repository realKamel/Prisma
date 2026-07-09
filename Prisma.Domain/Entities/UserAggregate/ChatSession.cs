using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.UserAggregate;

public sealed class ChatSession : IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid? UserId { get; private set; }
    public string Title { get; set; }
    public string SerializedSessionJson { get; private set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private ChatSession()
    {
    }

    public static ChatSession Create(Guid? userId, string title, string serializedSessionJson)
    {
        return new ChatSession
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Title = title,
            SerializedSessionJson = serializedSessionJson,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void Update(string serializedSessionJson)
    {
        SerializedSessionJson = serializedSessionJson;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}