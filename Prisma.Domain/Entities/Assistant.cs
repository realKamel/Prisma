using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Assistant : BaseEntity
{
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public string? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
