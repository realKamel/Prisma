namespace Prisma.Application.Common.DTOs;

public record PaginationParams
{
    private const int maxPageSize = 50;

    public int PageNumber { get; init; } = 1;

    private readonly int pageSize = 10;

    public int PageSize
    {
        get => pageSize;
        init => pageSize = value > maxPageSize ? maxPageSize : value;
    }
}