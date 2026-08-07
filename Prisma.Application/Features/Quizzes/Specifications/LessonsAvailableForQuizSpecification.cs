using System;
using System.Collections.Generic;
using System.Text;
using Ardalis.Specification;
using Prisma.Application.Features.Quizzes.Dtos;
using Prisma.Domain.Entities.LessonAggregate;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Prisma.Application.Features.Quizzes.Specifications;

public class LessonsAvailableForQuizSpecification
    : Specification<Lesson, LessonOptionDto>
{
    public LessonsAvailableForQuizSpecification()
    {
        Query
            .Where(l => l.QuizId == null)
            .Select(l => new LessonOptionDto
            {
                LessonId = l.Id,
                Title = l.Title ?? string.Empty
            });
    }
}
