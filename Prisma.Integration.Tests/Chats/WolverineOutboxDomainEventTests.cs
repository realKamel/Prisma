using JasperFx;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prisma.Application;
using Prisma.Application.Abstractions.Services.Chatting;
using Prisma.Domain.Common;
using Prisma.Domain.Entities.ChatAggregate;
using Prisma.Domain.Interfaces;
using Prisma.Domain.Repositories;
using Prisma.Infrastructure.Persistence;
using Prisma.Infrastructure.Persistence.Interceptors;
using Prisma.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace Prisma.Integration.Tests.Chats;

/// <summary>
/// Regression coverage for the real-time chat pipeline. Chat broadcasts only happen when the
/// domain events raised by <see cref="Conversation"/> are published, and they are only published
/// when the save goes through Wolverine's EF Core outbox. Persisting with a plain
/// <c>DbContext.SaveChangesAsync</c> keeps the rows but silently drops every domain event.
/// </summary>
public sealed class WolverineOutboxDomainEventTests : IAsyncLifetime
{
    private static readonly TimeSpan HandlerTimeout = TimeSpan.FromSeconds(30);

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg18")
        .WithDatabase("prisma_outbox_tests")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    private readonly RecordingChatHubService _chatHubService = new();

    private IHost? _host;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var connectionString = _postgres.GetConnectionString();

        _host = Host.CreateDefaultBuilder()
            .UseEnvironment(Environments.Development)
            .ConfigureServices(services =>
            {
                // Mirrors DependenciesInjection.AddPersistenceConfig: the outbox-aware registration
                // is what makes Wolverine scrape domain events during SaveChanges, and the audit
                // interceptor is what stamps the required CreatedAt/UpdatedAt columns.
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.AddSingleton<AuditInterceptor>();

                services.AddDbContextWithWolverineIntegration<AppDbContext>(
                    (serviceProvider, options) =>
                        options
                            .UseNpgsql(connectionString, npgsql => npgsql.UseVector())
                            .AddInterceptors(
                                serviceProvider.GetRequiredService<AuditInterceptor>()
                            )
                );

                services.AddSingleton<IChatHubService>(_chatHubService);
                services.AddSingleton<IChatPresenceTracker, OfflineChatPresenceTracker>();
                services.AddSingleton<IUnreadBadgeStore, InMemoryUnreadBadgeStore>();
                services.AddScoped<IConversationRepository, ConversationRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            })
            .UseWolverine(options =>
            {
                options.Discovery.IncludeAssembly(typeof(ApplicationAssemblyMarker).Assembly);

                options.PublishDomainEventsFromEntityFrameworkCore<IDomainEventEnumerable>(entity =>
                    entity.DomainEvents
                );

                options.PersistMessagesWithPostgresql(connectionString);
                options.Policies.UseDurableOutboxOnAllSendingEndpoints();
                options.AutoBuildMessageStorageOnStartup = AutoCreate.CreateOrUpdate;
            })
            .Build();

        await using (var scope = _host.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        await _postgres.DisposeAsync();
    }

    [Fact]
    public void outbox_is_registered_for_the_application_db_context()
    {
        using var scope = TestHost.Services.CreateScope();

        var outbox = scope.ServiceProvider.GetRequiredService<IDbContextOutbox<AppDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // UnitOfWork resolves the DbContext through the outbox, so both must be the same instance
        // or the scrape/save/flush would happen on a different unit of work than the repositories.
        Assert.Same(dbContext, outbox.DbContext);
    }

    [Fact]
    public void application_db_context_model_contains_the_wolverine_envelope_storage()
    {
        using var scope = TestHost.Services.CreateScope();

        var entityNames = scope
            .ServiceProvider.GetRequiredService<AppDbContext>()
            .Model.GetEntityTypes()
            .Select(entityType => entityType.ClrType.Name)
            .ToList();

        // Only mapped when the DbContext is registered through AddDbContextWithWolverineIntegration
        // and Wolverine persists messages with Postgres; without them the outbox cannot store envelopes.
        Assert.Contains("OutgoingMessage", entityNames);
        Assert.Contains("IncomingMessage", entityNames);
    }

    [Fact]
    public async Task saving_through_the_unit_of_work_runs_the_message_sent_domain_event_handler()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var conversationId = Guid.Empty;
        var messageId = Guid.Empty;

        await using (var scope = TestHost.Services.CreateAsyncScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var conversations = unitOfWork.GetOrCreateRepository<Conversation, Guid>();

            var conversationResult = Conversation.CreateDirect(senderId, recipientId);
            Assert.True(conversationResult.IsSuccess);

            var conversation = conversationResult.Value;
            conversationId = conversation.Id;

            var messageResult = conversation.AddMessage(senderId, "hello from the outbox");
            Assert.True(messageResult.IsSuccess);

            messageId = messageResult.Value.Id;

            conversations.Add(conversation);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        var broadcast = await _chatHubService.WaitForMessageAsync(messageId, HandlerTimeout);

        Assert.Equal(messageId, broadcast.Id);
        Assert.Equal(conversationId, broadcast.ConversationId);
        Assert.Equal(senderId, broadcast.SenderId);
        Assert.Equal("hello from the outbox", broadcast.Content);

        await using var verificationScope = TestHost.Services.CreateAsyncScope();
        var dbContext = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var persisted = await dbContext.Set<Message>().SingleAsync(message => message.Id == messageId);

        Assert.Equal(conversationId, persisted.ConversationId);
    }

    [Fact]
    public async Task saving_twice_in_the_same_scope_does_not_rebroadcast_the_same_message()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var messageId = Guid.Empty;

        await using (var scope = TestHost.Services.CreateAsyncScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var conversations = unitOfWork.GetOrCreateRepository<Conversation, Guid>();

            var conversation = Conversation.CreateDirect(senderId, recipientId).Value;
            var message = conversation.AddMessage(senderId, "only once").Value;
            messageId = message.Id;

            conversations.Add(conversation);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);

            // A second save in the same request scope must not replay already flushed events.
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        await _chatHubService.WaitForMessageAsync(messageId, HandlerTimeout);

        await Task.Delay(TimeSpan.FromSeconds(1));

        var matching = _chatHubService.Messages.Count(message => message.Id == messageId);

        Assert.Equal(1, matching);
    }

    private IHost TestHost =>
        _host ?? throw new InvalidOperationException("The test host was not built.");
}
