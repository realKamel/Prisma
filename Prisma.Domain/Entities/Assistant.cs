using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Assistant : Audit
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
