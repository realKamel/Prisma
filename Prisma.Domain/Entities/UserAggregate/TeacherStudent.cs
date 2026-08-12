using System;
using System.Collections.Generic;
using System.Text;
using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.UserAggregate;

public class TeacherStudent : BaseEntity
{
    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; }

    public  bool IsKicked { get; set; } 
    public Guid? KickedByUserId { get; set; }
    public DateTimeOffset? KickedAt { get; set; } 

}
