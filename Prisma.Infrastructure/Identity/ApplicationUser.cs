using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Common;

namespace Prisma.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>, IAuditable
{
    public Guid DomainUserId { get; set; }

    public bool IsOnline { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset? RefreshTokenExpiry { get; set; }

    public string? PasswordResetCode { get; set; }

    public bool PasswordResetConfirmed { get; set; }

    public DateTimeOffset? PasswordResetCodeExpiry { get; set; }

    public int ResetPasswordCodeAttemptCount { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Guid? DeletedBy { get; set; }

    public bool IsDeleted { get; set; }

    public List<IdentityUserClaim<Guid>> Claims { get; set; } = [];
}