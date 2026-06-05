using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; } 
    public bool IsActive { get; set; }
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Admin> Admins{ get; set; } = new List<Admin>();
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public virtual ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
}
