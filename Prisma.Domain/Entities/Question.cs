using System.Collections.Generic;
using Prisma.Domain.Entities;

namespace Prisma.Domain.Entities;

public class Question : BaseEntity
{
    public string? Type { get; set; } 
    public string? Title { get; set; } 

    public virtual ICollection<Choice> Choices { get; set; } = new List<Choice>();
    public virtual ICollection<TrueAndFalse> TrueAndFalseOptions { get; set; } = new List<TrueAndFalse>();
    public virtual ICollection<Written> WrittenOptions { get; set; } = new List<Written>();
    public virtual ICollection<AttemptAnswer> AttemptAnswers { get; set; } = new List<AttemptAnswer>();
}