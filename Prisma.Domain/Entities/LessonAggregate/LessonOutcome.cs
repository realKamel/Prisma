using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.LessonAggregate;

public class LessonOutcome : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }
}