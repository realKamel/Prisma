namespace Prisma.Infrastructure.ExternalServices.BunnyApi.Core;

public record BunnyCoreErrorResponse(string ErrorKey, string Field, string Message);
