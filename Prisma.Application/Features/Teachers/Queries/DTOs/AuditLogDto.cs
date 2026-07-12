namespace Prisma.Application.Features.Teachers.Queries.DTOs;

public record AuditLogDto(int Id, string UserEmail, string Action, string TableName, DateTimeOffset? CreatedAt);
