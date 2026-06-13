using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Entities.LessonAggregate;
using Prisma.Domain.Specifications;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

public class LessonWithDetailsSpecification : BaseSpecification<Lesson>
{
    public LessonWithDetailsSpecification(int lessonId)
        : base()
    {

        AddCriteria(l => l.Id == lessonId);

        AddInclude(l => l.Sections);
        AddInclude(l => l.Enrollments);
        AddInclude(l => l.Outcomes);
        AddInclude(l => l.Quizzes);
        AddInclude(l => l.Prerequisite);

        AddInclude("Quizzes.Attempts");
    }
}