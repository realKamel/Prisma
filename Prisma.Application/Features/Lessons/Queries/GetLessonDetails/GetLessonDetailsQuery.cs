using MediatR;
using Ardalis.Result;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

public record GetLessonDetailsQuery(int LessonId) : IRequest<Result<LessonDetailsDto>>;

public record LessonDetailsDto
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int ChaptersCount { get; set; }
    public int StudentsCount { get; set; }
    public decimal Price { get; set; }
    public int ValidityDays { get; set; }
    public string AboutText { get; set; } = string.Empty;
    public List<string> Outcomes { get; set; } = [];
    public List<PrerequisiteDto> Prerequisites { get; set; } = [];
    public List<ChapterDto> Chapters { get; set; } = [];
}

public record PrerequisiteDto(string Title, bool IsDone);

public record ChapterDto(int Id, string Title, string Duration, bool IsPreview);