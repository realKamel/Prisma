using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Students.Queries.GetStudentDashboardQuery;

public record GetStudentDashboardQuery : IRequest<Result<GetStudentDashboardResponse>>;

public class GetStudentDashboardResponse
{
    public StudentDto Student { get; set; } = null!;
    public StreakDto Streak { get; set; } = null!;
    public NextLessonDto? NextLesson { get; set; }
    public List<LessonCardDto> Lessons { get; set; } = [];
    public StatsDto Stats { get; set; } = null!;
}

public class StudentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string GradeLabel { get; set; } = string.Empty;
}

public class StreakDto
{
    public int Count { get; set; }
}

public class NextLessonDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherInitial { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public int CurrentChapter { get; set; }
    public int TotalChapters { get; set; }
    public string PlayerUrl { get; set; } = string.Empty;
    public string DetailUrl { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
}

public class LessonCardDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherInitial { get; set; } = string.Empty;

    // تم التعديل إلى TimeSpan بناءً على طلبك 🌟
    public TimeSpan Duration { get; set; }

    public LessonStatus Status { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public int? ExpiresInDays { get; set; }
    public string PlayerUrl { get; set; } = string.Empty;
}

public class StatsDto
{
    public int PurchasedLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int StudyHours { get; set; }
    public int TopQuizScore { get; set; }
}

public enum LessonStatus
{
    New,
    Progress,
    Done,
    Warn,
    Expired
}