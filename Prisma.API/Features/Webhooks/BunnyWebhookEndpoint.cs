using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Encryption;
using Prisma.Application.Abstractions.Persistence;
using Prisma.Application.Features.Webhooks.Bunny.Commands;
using Prisma.Domain.Aggregates.VideoLibraryAggregate;
using Wolverine;
using Wolverine.Http;
using ZiggyCreatures.Caching.Fusion;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Prisma.API.Features.Webhooks;

public static class BunnyWebhookEndpoint
{
    private sealed record CachedLibrary(long? ProviderLibraryId, string? ReadOnlyApiKey);

    private static readonly FusionCacheEntryOptions LibraryCacheOptions = new()
    {
        Duration = TimeSpan.FromHours(1),
    };

    //TODO: versioning again can break things
    [WolverinePost("/api/v1/webhooks/bunny/stream/{providerLibraryId:long}")]
    public static async Task<IResult> Handle(
        HttpContext context,
        long providerLibraryId,
        IWebhookSignatureVerifier signatureVerifier,
        IReadonlyAppDbContext appDbContext,
        IFusionCacheProvider cacheProvider,
        IMessageBus bus,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        var logger = loggerFactory.CreateLogger(
            "Prisma.API.Features.Webhooks.BunnyWebhookEndpoint"
        );

        if (!WebhooksHelper.IsJsonContentType(context.Request.ContentType))
        {
            return Results.BadRequest();
        }

        if (context.Request.ContentLength is > WebhooksHelper.MaxBodyBytes)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        byte[] rented = ArrayPool<byte>.Shared.Rent(WebhooksHelper.MaxBodyBytes);
        int bytesRead = 0;
        try
        {
            while (bytesRead < WebhooksHelper.MaxBodyBytes)
            {
                int read = await context.Request.Body.ReadAsync(
                    rented.AsMemory(bytesRead, WebhooksHelper.MaxBodyBytes - bytesRead),
                    cancellationToken
                );

                if (read == 0)
                {
                    break;
                }

                bytesRead += read;
            }

            if (bytesRead == WebhooksHelper.MaxBodyBytes)
            {
                Memory<byte> probe = new byte[1];
                if (await context.Request.Body.ReadAsync(probe, cancellationToken) > 0)
                {
                    return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
                }
            }

            var rawBody = rented.AsMemory(0, bytesRead);
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();

            IFusionCache cache = cacheProvider.GetCache("wasla_cache");

            var library = await GetLibraryAsync(
                appDbContext,
                cache,
                providerLibraryId,
                cancellationToken
            );

            if (library is null)
            {
                BunnyWebhookLogs.UnknownLibrary(logger, providerLibraryId, remoteIp);
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(library.ReadOnlyApiKey))
            {
                BunnyWebhookLogs.MissingSigningKey(logger, providerLibraryId);
                return Results.UnprocessableEntity();
            }

            if (
                !signatureVerifier.Verify(
                    rawBody,
                    signature: context.Request.Headers["X-BunnyStream-Signature"].ToString(),
                    version: context.Request.Headers["X-BunnyStream-Signature-Version"].ToString(),
                    algorithm: context
                        .Request.Headers["X-BunnyStream-Signature-Algorithm"]
                        .ToString(),
                    library.ReadOnlyApiKey
                )
            )
            {
                BunnyWebhookLogs.SignatureVerificationFailed(logger, providerLibraryId, remoteIp);
                return Results.Unauthorized();
            }

            VideoWebhookPayload? payload;

            try
            {
                payload = JsonSerializer.Deserialize<VideoWebhookPayload>(rawBody.Span);
            }
            catch (JsonException ex)
            {
                BunnyWebhookLogs.InvalidJson(logger, providerLibraryId, remoteIp, ex);
                return Results.BadRequest();
            }

            if (payload is null)
            {
                BunnyWebhookLogs.MalformedPayload(logger, providerLibraryId, remoteIp);
                return Results.BadRequest();
            }

            // The signature covers the body, but the body still has to agree with the route.
            if (payload.VideoLibraryId != providerLibraryId)
            {
                BunnyWebhookLogs.LibraryIdMismatch(
                    logger,
                    providerLibraryId,
                    payload.VideoLibraryId,
                    payload.EventId
                );

                return Results.BadRequest();
            }

            if (payload.IsLiveStreamWebhook)
            {
                // Live stream callbacks share this endpoint but this flow only tracks uploads.
                BunnyWebhookLogs.LiveStreamEventIgnored(logger, payload.EventId, providerLibraryId);
                return Results.Ok();
            }

            if (payload.EventId == Guid.Empty || payload.VideoGuid == Guid.Empty)
            {
                BunnyWebhookLogs.MalformedPayload(logger, providerLibraryId, remoteIp);
                return Results.BadRequest();
            }

            var command = new ProcessBunnyWebhookCommand
            {
                VideoLibraryId = providerLibraryId,
                Status = payload.Status,
                VideoGuid = payload.VideoGuid,
                EventId = payload.EventId,
                Reason = payload.Reason,
                Timestamp = payload.Timestamp,
            };

            var result = await bus.InvokeAsync<Result>(command, cancellationToken);

            if (!result.IsSuccess)
            {
                // Business outcomes are acknowledged by the handler, so this is unexpected.
                BunnyWebhookLogs.CommandRejected(
                    logger,
                    payload.Status.ToString(),
                    payload.EventId,
                    providerLibraryId
                );

                return Results.Unauthorized();
            }

            return Results.Ok();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(rented.AsSpan(0, bytesRead));
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static async Task<CachedLibrary?> GetLibraryAsync(
        IReadonlyAppDbContext appDbContext,
        IFusionCache cache,
        long providerLibraryId,
        CancellationToken cancellationToken
    )
    {
        var cacheKey =
            $"bunny:library:{providerLibraryId.ToString("G", CultureInfo.InvariantCulture)}";

        var cached = await cache.TryGetAsync<CachedLibrary>(cacheKey, token: cancellationToken);

        if (cached.HasValue)
        {
            return cached.Value;
        }

        // Only positive lookups are cached: an unknown library may still be provisioning and
        // must not stay rejected for the lifetime of the entry.
        var library = await appDbContext
            .Entity<VideoLibrary, int>()
            .Where(x => x.ProviderLibraryId == providerLibraryId)
            .Select(x => new CachedLibrary(x.ProviderLibraryId, x.ReadOnlyApiKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (library is not null)
        {
            await cache.SetAsync(
                cacheKey,
                library,
                LibraryCacheOptions,
                tags: ["bunny:library", cacheKey],
                token: cancellationToken
            );
        }

        return library;
    }
}
