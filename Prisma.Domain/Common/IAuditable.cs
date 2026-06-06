namespace Prisma.Domain.Common;

public interface IAuditable
{
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? CreatedBy { get; set; }
    Guid? UpdatedBy { get; set; }
    Guid? DeletedBy { get; set; }
    bool IsDeleted { get; set; }
}