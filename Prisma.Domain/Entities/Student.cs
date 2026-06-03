using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Student : BaseEntity
{
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? ParentPhoneNumber { get; set; }

    public string? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public string? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
}

