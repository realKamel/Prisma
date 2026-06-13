using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Enums;

namespace Prisma.Application.Features.LessonCatalog.Queries;

public class LessonCatalogDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    public decimal Price { get; init; }

    public LessonCatalogStatus Status { get; init; }

    public string? PrerequisiteLabel { get; init; }

    public DateTimeOffset? ExpiredDate { get; init; }
}
