using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class QuizStudentsProjection
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public decimal TotalDegree { get; init; }

    public QuizScope Scope { get; init; }
    public int? LessonId { get; init; }
    public int? AcademicYearId { get; init; }
    public DateTimeOffset? DueDate { get; init; }

    public List<QuizAttemptProjection> Attempts { get; init; } = [];
}

public class QuizAttemptProjection
{
    public Guid StudentId { get; init; }

    public QuizAttemptStatus Status { get; init; }

    public decimal? Degree { get; init; }

    public DateTimeOffset? SubmittedAt { get; init; }

    public int TabSwitchCount { get; init; }

    public int CopyPasteAttemptCount { get; init; }

    public int PendingWrittenCount { get; init; }
}