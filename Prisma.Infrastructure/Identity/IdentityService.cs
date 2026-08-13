using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<IdentityResult> CreateAsync(User user, string password) => await userManager.CreateAsync(user, password);

    public async Task<IdentityResult> AddToRoleAsync(User user, string role) => await userManager.AddToRoleAsync(user, role);

    public async Task<User?> FindByEmailAsync(string email) => await userManager.FindByEmailAsync(email);

    public async Task<List<TUser>> GetUsers<TUser>(CancellationToken cancellationToken) where TUser : User
    {
        return await userManager.Users
            .OfType<TUser>()
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await userManager
            .Users
            .Include(u => u.Roles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> FindByPhoneNumberAsync(string number, CancellationToken cancellationToken)
    {
        return await userManager
            .Users
            .SingleOrDefaultAsync(u => u.PhoneNumber == number, cancellationToken);
    }

    public async Task<User?> FindByEmailOrPhoneAsync(
        string? email,
        string? phone,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var normalizedEmail = !string.IsNullOrWhiteSpace(email)
            ? userManager.NormalizeEmail(email)
            : null;

        return await userManager.Users
            .AsSplitQuery()
            .Include(u => u.Claims)
            .Include(u => u.Roles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(u =>
                (normalizedEmail != null && u.NormalizedEmail == normalizedEmail) ||
                (!string.IsNullOrWhiteSpace(phone) && u.PhoneNumber == phone),
                cancellationToken: cancellationToken);
    }

    public async Task<IdentityResult> DeleteAsync(User user) => await userManager.DeleteAsync(user);

    public async Task<IList<Claim>> GetClaimsAsync(User user) => await userManager.GetClaimsAsync(user);

    public async Task<IdentityResult> UpdateAsync(User user) => await userManager.UpdateAsync(user);

    public async Task<IList<string>> GetRolesAsync(User user) => await userManager.GetRolesAsync(user);

    public async Task<bool> CheckPasswordAsync(User user, string password) => await userManager.CheckPasswordAsync(user, password);

    public async Task<IdentityResult> AddClaimsAsync(User user, IEnumerable<Claim> claims)
    {
        return await userManager.AddClaimsAsync(user, claims);
    }

    public async Task<IdentityResult> RemoveClaimsAsync(User user, IEnumerable<Claim> claims)
    {
        return await userManager.RemoveClaimsAsync(user, claims);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> SetPhoneNumberAsync(User user, string phoneNumber)
    {
        return await userManager.SetPhoneNumberAsync(user, phoneNumber);
    }
    public async Task<IdentityResult> SetUserNameAsync(User user, string userName)
    {
        return await userManager.SetUserNameAsync(user, userName);
    }
    public async Task<IdentityResult> SetEmailAsync(User user, string email)
    {
        return await userManager.SetEmailAsync(user, email);
    }
    public async Task<string> GeneratePasswordResetTokenAsync(User user) => await userManager.GeneratePasswordResetTokenAsync(user);
    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }
}