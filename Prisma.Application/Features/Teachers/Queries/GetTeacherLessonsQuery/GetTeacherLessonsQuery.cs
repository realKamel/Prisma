using System;
using System.Collections.Generic;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Teachers.Queries.GetTeacherLessons;
public record GetTeacherLessonsQuery : IRequest<Result<List<TeacherLessonDto>>>;

public class TeacherLessonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Students { get; set; }
    public string Status { get; set; } = "active"; 
}