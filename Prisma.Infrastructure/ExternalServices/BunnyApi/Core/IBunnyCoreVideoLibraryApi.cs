using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.AddVideoLibrary;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.GetVideoLibraries;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.GetVideoLibrary;
using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.UpdateVideoLibrary;
using Refit;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core;

public interface IBunnyCoreVideoLibraryApi
{
    /// <summary>
    /// Retrieves a paginated list of Video Libraries on the account.
    /// </summary>
    /// <param name="page">The page number to retrieve. Minimum value is 1. (Default: 1)</param>
    /// <param name="perPage">The number of items per page. Must be between 5 and 1000. (Default: 1000)</param>
    /// <param name="search">Optional search term to filter the results by library name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A paginated list of VideoLibraryModel objects.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if page or perPage are out of API bounds.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error status code.</exception>
    [Get("/videolibrary")]
    Task<GetVideoLibrariesResponse> GetVideoLibraries(
        [Query] int page,
        [Query] int perPage,
        [Query] string? search,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Creates a new Video Library on the account.
    /// </summary>
    /// <param name="request">The configuration details for the new video library. The 'Name' property is required.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The details of the newly created video library, including generated IDs and API keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error status code (e.g., 400 Bad Request, 401 Unauthorized).</exception>
    [Post("/videolibrary")]
    Task<AddVideoLibraryResponse> AddVideoLibrary(
        [Body] AddVideoLibraryRequest request,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves the detailed configuration of a specific Video Library by its ID.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the Video Library to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The requested VideoLibraryModel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error status code (e.g., 401 Unauthorized, 403 Forbidden, 404 Not Found).</exception>
    [Get("/videolibrary/{id}")]
    Task<GetVideoLibraryResponse> GetVideoLibrary(
        [AliasAs("id")] long libraryId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Updates an existing Video Library.
    /// </summary>
    /// <param name="libraryId">The ID of the Video Library to update.</param>
    /// <param name="updateModel">The properties to update. Only non-null properties will be applied.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated VideoLibraryModel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0.</exception>
    /// <exception cref="ArgumentNullException">Thrown if updateModel is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error status code.</exception>
    [Post("/videolibrary/{id}")]
    Task<UpdateVideoLibraryResponse> UpdateVideoLibrary(
        [AliasAs("id")] long libraryId,
        [Body] UpdateVideoLibraryRequest updateModel,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Permanently deletes a Video Library and all its associated videos.
    /// </summary>
    /// <param name="libraryId">The unique identifier of the Video Library to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if libraryId is less than or equal to 0.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error status code (e.g., 401 Unauthorized, 404 Not Found).</exception>
    [Delete("/videolibrary/{id}")]
    Task DeleteVideoLibrary([AliasAs("id")] long libraryId, CancellationToken cancellationToken);
}
