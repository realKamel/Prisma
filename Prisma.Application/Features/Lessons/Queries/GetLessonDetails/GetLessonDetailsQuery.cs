using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Prisma.Application.Common.Responses.Generic;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Features.Lessons.Queries.GetLessonDetails;

public record GetLessonDetailsQuery(int LessonId) : IRequest<Result<LessonDetailsDto>>;
public class LessonDetailsDto
{

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public int ChaptersCount { get; set; }
    public int StudentsCount { get; set; }
    public decimal Price { get; set; }
    public int ValidityDays { get; set; }
    public string AboutText { get; set; } = string.Empty;
    public List<string> Outcomes { get; set; } = [];
    public List<PrerequisiteDto> Prerequisites { get; set; } = [];
    public List<ChapterDto> Chapters { get; set; } = [];
}

// الـ Records الخفيفة ومجهزة بالـ Types الصح للـ JSON
public record PrerequisiteDto(string Title, bool IsDone);
public record ChapterDto(int Id, string Title, string Duration, bool IsPreview);