using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class GradeWrittenAnswersResultDto
{
    public string Status { get; set; } = string.Empty;
    public bool HeldForSecurityReview { get; set; }
}
