namespace Prisma.Application.Features.Quizzes.Dtos;

public class GradingListResponseDto
{
    public List<GradingListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
