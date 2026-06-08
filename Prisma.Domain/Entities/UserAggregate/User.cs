using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Common;

namespace Prisma.Domain.Entities.UserAggregate;

public class User : IdentityUser<Guid>, IAuditable
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsBlocked { get; set; }

    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiry { get; set; }

    public string? PasswordResetCode { get; set; }
    public DateTimeOffset? PasswordResetCodeExpiry { get; set; }
    public int ResetPasswordCodeAttemptCount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}