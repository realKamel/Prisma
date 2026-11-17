using System.ComponentModel.DataAnnotations;

namespace Prisma.Application.Common.Options;

public sealed class ApplicationUrlsOptions
{
    public const string SectionName = "ApplicationUrls";

    [Required, Url]
    public required string BackendUrl { get; set; }

    [Required, Url]
    public required string FrontendUrl { get; set; }

    /// <summary>
    /// Composes an absolute backend URL from a relative path.
    /// </summary>
    public string Backend(string path) => $"{BackendUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    public string Frontend(string path) => $"{FrontendUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}
