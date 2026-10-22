namespace Prisma.Application.Features.Chats;

public static class ChatGroups
{
    public static string Conversation(Guid conversationId) => $"conversation:{conversationId:N}";
}