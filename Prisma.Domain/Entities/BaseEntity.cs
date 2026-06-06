namespace Prisma.Domain.Entities;

public class BaseEntity
{
    public int Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}