using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class SubmitQuizResultDto
{
    public string Status { get; set; } = string.Empty; // "submitted" | "graded"
    public decimal? Score { get; set; }
    public decimal TotalDegree { get; set; }
}