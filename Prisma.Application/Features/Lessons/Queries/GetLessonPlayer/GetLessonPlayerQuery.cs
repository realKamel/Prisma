using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonPlayer;

public record GetLessonPlayerQuery(int id) : IRequest<Result<LessonPlayerResult>>;

public class LessonPlayerResult
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public string ParentTitle { get; set; } = string.Empty;
    public bool IsPurchased { get; set; }
    public string PurchaseLabel { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string TeacherInitial { get; set; } = string.Empty;
    public string ExpiryLabel { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoPoster { get; set; } = string.Empty;
    public AboutTab AboutTab { get; set; } = new();
    public List<Material> Materials { get; set; } = new();
    public Quiz? Quiz { get; set; }
    public Assignment? Assignment { get; set; }
    public List<SectionPlayer> Sections { get; set; } = new();
}

public class AboutTab
{
    public string Description { get; set; } = string.Empty;
    public List<string> Objectives { get; set; }
}

public class Material
{
    public string Title { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public class Quiz
{
    public string Id { get; set; } = string.Empty;
    public int QuestionsCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
}

public class Assignment
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
}
public class SectionPlayer
{
    public string Title { get; set; } = string.Empty;
    public List<SectionItem> Items { get; set; } = new();
}

public class SectionItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}