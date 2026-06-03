using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Admin : BaseEntity
{
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
