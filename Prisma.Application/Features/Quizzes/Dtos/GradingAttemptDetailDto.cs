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
    public decimal PenaltyScore { get; set; }
    public string Status { get; set; } = string.Empty; // "submitted" | "graded"
    //public bool HeldForSecurityReview { get; set; }
    //public bool HeldForManualGrading { get; set; }
    public List<GradingQuestionDto> Questions { get; set; } = new();
}
