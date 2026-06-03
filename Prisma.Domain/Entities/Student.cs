using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Prisma.Domain.Entities;
public class Student : Audit
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? ParentPhoneNumber { get; set; }

    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public int AcademicYearId { get; set; }
    //public AcademicYear? AcademicYear { get; set; }
}

