using Prisma.API.Features.Chats;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Infrastructure.RealTime;

namespace Prisma.API.Extensions;

public static partial class WebAppHelper
{
    internal static void AddChatRealTimeServices(this IServiceCollection services)
    {
        // Binds the infrastructure bridge to the concrete presentation-layer hub.
        services.AddScoped<IChatHubService, ChatHubService<ChatHub>>();
    }

    internal static void MapChatHub(this WebApplication app)
    {
        app.MapHub<ChatHub>("api/hubs/chat");
    }
}
