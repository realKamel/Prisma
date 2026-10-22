namespace Prisma.Application.Features.Chats.Dtos;

public sealed record PresenceChangedDto(Guid UserId, bool IsOnline, DateTimeOffset OccurredAt);