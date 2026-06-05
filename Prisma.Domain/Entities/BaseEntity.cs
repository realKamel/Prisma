using System;
using System.Collections.Generic;
using System.Text;

namespace Prisma.Domain.Entities;

public class BaseEntity
{
    public required int Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt{ get; set; }
    public DateTime? DeletedAt{ get; set; }
    public string? CreatedBy{ get; set; }
    public string? ModifiedBy{ get; set; }
    public string? DeletedBy{ get; set; }
    public bool IsDeleted{ get; set; }

}
