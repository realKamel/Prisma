namespace Prisma.Application.Features.Teachers.Queries.DTOs;

public record AuditLogDto(
    long Id,
    string UserEmail,
    string Action,
    string TableName,
    DateTimeOffset? CreatedAt
);
