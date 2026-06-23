using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class GradingAttemptDetailDto
{
    public int AttemptId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public DateTimeOffset? SubmittedAt { get; set; }
    public decimal TotalDegree { get; set; }
    public decimal? Score { get; set; }
    public string Status { get; set; } = string.Empty; // "submitted" | "graded"
    public List<GradingQuestionDto> Questions { get; set; } = new();
}
