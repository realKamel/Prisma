namespace Prisma.API.Features.Chats.Requests;

public sealed record MarkAsReadRequest(Guid LastMessageId);