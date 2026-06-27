using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Assistants.Queries.GetAssistantLessons;
public record GetAssistantLessonsQuery : IRequest<Result<List<AssistantLessonDto>>>;

public class AssistantLessonDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StudentsCount { get; set; }
    public int ChaptersCount { get; set; }
    public DateTimeOffset? LastUpdatedAt {get; set;}
    public string Status { get; set; } = "active"; 
}