using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class GradingChoiceDto
{
    public int ChoiceId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
