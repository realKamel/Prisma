using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Teacher : Audit
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string Subject { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; } = new List<Student>();

    public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
}
