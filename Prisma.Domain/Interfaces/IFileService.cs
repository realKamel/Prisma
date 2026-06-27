using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Prisma.Application.Abstractions.Services;

public interface IFileService
{
    
    Task<string> UploadFileAsync(IFormFile file, string subFolder, CancellationToken cancellationToken);
}