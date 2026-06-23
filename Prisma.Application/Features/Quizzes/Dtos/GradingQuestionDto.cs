using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class GradingQuestionDto
{
    public int QuestionId { get; set; }
    public int AnswerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Degree { get; set; }       // max degree for this question
    public decimal? Score { get; set; }        // null = not graded yet (written only)
    public bool? IsCorrect { get; set; }       // null = not graded yet (written only)

    // MCQ / TrueFalse
    public List<GradingChoiceDto>? Choices { get; set; }
    public int? SelectedChoiceId { get; set; }

    // Written
    public string? TextAnswer { get; set; }
    public string? ModelAnswer { get; set; }   // correct answer set by teacher
}
