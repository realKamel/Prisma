using Prisma.Infrastructure.ExternalServices.BunnyApi.Core.PullZoneApi.GetPullZone;
using Refit;

namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core;

public interface IBunnyCorePullZoneApi
{
    /// <summary>
    /// Retrieves the detailed configuration of a specific Pull Zone by its ID.
    /// </summary>
    /// <param name="id">The unique ID of the Pull Zone.</param>
    /// <param name="includeCertificate">Determines if the result hostnames should contain the SSL certificate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The requested PullZoneModel.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if id is less than or equal to 0.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an error status code (401, 404, 500).</exception>
    [Get("/pullzone/{id}")]
    Task<GetPullZoneResponse> GetPullZone(
        [AliasAs("id")] long id,
        [Query] bool? includeCertificate = null,
        CancellationToken cancellationToken = default
    );
}
