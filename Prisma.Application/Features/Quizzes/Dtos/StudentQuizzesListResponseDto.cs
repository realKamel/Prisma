using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Application.Features.Quizzes.Dtos;

public class StudentQuizzesListResponseDto
{
    public StudentQuizzesStatsDto Stats { get; set; } = new();
    public List<StudentQuizListItemDto> Items { get; set; } = new();
  
}
