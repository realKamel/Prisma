using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;

public record GetLessonPlayerQuery(int id) : IRequest<Result<LessonPlayerResult>>;

public class LessonPlayerResult
{
    public int Id { get; set; } //
    public string Title { get; set; } = string.Empty; //
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty; //

    public string Description { get; set; } = string.Empty; //
    public string Teacher { get; set; } = string.Empty; //

    public int ValidityDays { get; set; } //

    public string VideoPoster { get; set; } = string.Empty;
    public List<MaterialDto> Materials { get; set; } = new(); //
    public QuizDto? Quiz { get; set; } //
    public AssignmentDto? Assignment { get; set; } //
    public List<SectionDto> Sections { get; set; } = new(); //

    public List<string> Outcomes { get; set; } = new(); //


}



public class MaterialDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public class QuizDto
{
    public string Id { get; set; } = string.Empty;
    public int QuestionsCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
}

public class AssignmentDto
{
    public string Id { get; set; } = string.Empty;
    public string ContentURL { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
}


public class SectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty; 
    public bool IsCompleted { get; set; }
    public string? ContentUrl { get; set; }
    public int Progress { get; set; }
}