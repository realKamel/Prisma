using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class CreateQuizQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Degree { get; set; }
    public List<CreateQuizChoiceDto>? Choices { get; set; } 
    public string? ModelAnswer { get; set; }        
}
public class CreateQuizChoiceDto
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class CreateQuizResultDto
{
    public int QuizId { get; set; }
    public decimal TotalDegree { get; set; }
}