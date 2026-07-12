using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonExpired;

public record GetLessonExpiredQuery(int LessonId) : IRequest<Result<LessonExpiredDto>>;
public class LessonExpiredDto
{

    public int Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; }= string.Empty;

    public int ChaptersCount { get; set; }
    public decimal Price { get; set; }

    public int MaterialsCount { get; set; }

    public double totalprogress { get; set; }

    public decimal Degree { get; set; }


    public DateTimeOffset? ExpiredDate { get; set; }
    public int ValidityDays { get; set; }
    public List<ChapterDto> Chapters { get; set; } = [];
}

public record ChapterDto(int Id, string Title, string Duration);

