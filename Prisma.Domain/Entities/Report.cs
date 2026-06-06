using System;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Report : BaseEntity
{
    public string? Content { get; set; } 
    public DateTime Date { get; set; }
    
    public Guid StudentId { get; set; } 
    public Student? Student { get; set; } 
}