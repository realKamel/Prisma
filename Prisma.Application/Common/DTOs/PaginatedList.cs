namespace Prisma.Application.Common.DTOs;

public record PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
        TotalCount = totalCount < 0 ? 0 : totalCount;
        Items = (IReadOnlyCollection<T>)items.ToList().AsReadOnly() ?? [];
    }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization (System.Text.Json / Newtonsoft)
    /// </summary>
    public PaginatedList()
    {
        Items = [];
    }
}