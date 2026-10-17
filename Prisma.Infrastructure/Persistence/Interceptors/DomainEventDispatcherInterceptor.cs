using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Polly;
using Polly.Retry;
using Prisma.Domain.Common;

namespace Prisma.Infrastructure.Persistence.Interceptors;

internal sealed class DomainEventDispatcherInterceptor(IPublisher publisher)
    : SaveChangesInterceptor
{
    private readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(
            new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            }
        )
        .Build();

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var entitiesWithEvents = eventData
            .Context.ChangeTracker.Entries<IDomainEventEnumerable>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Wrap publishing in a resilient retry policy
        foreach (var domainEvent in domainEvents)
        {
            await _retryPipeline.ExecuteAsync(
                async (currentEvent, token) => await publisher.Publish(currentEvent, token),
                state: domainEvent,
                cancellationToken
            );
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
