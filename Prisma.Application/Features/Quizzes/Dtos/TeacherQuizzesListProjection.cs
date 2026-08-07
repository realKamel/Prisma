using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class TeacherQuizzesListProjection
{
    public int QuizId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TimeSpan TimeInMinutes { get; set; }
    public decimal TotalDegree { get; set; }
    public DateTimeOffset? AvailableFrom { get; set; }
    public DateTimeOffset? DueDate { get; set; }

    public int QuestionsCount { get; set; }

    public int PendingGradingCount { get; set; }
    public int SubmittedCount { get; set; }

    public decimal? AverageDegree { get; set; }

    public bool HasAttempts { get; set; }
    public bool HasUngradedAttempts { get; set; }
}