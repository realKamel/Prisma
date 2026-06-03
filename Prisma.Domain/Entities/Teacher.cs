using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Teacher : BaseEntity
{
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string Subject { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
    public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
}
