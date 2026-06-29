namespace Prisma.Application.Features.Assignments.Dtos;

public class AssignmentSubmissionsResponseDto
{
    public List<AssignmentSubmissionListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
