using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.SharedDtos;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;

/// <summary>
/// The added video library (Returned on 201 Created).
/// </summary>
public sealed record AddVideoLibraryResponse : VideoLibraryModel;
