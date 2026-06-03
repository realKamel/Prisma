using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string? FullName { get; set; } 
    public string? Role { get; set; }
    public bool IsActive { get; set; }
}
