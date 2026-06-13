using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.UserAggregate;

public class Role : IdentityRole<Guid>, IAuditable
{
    public Role() : base() { }
    public Role(string roleName) : base(roleName) { }

    public string? Description { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}