using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        return await userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        return await userManager.AddToRoleAsync(user, role);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<List<TUser>> GetUsers<TUser>(CancellationToken cancellationToken) where TUser : User
    {
        return await userManager.Users
            .OfType<TUser>()
            .Where(u => !u.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await userManager
            .Users
            .Where(u => (userId == u.Id && !u.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> FindByPhoneNumberAsync(string number, CancellationToken cancellationToken)
    {
        return await userManager
            .Users
            .Where(u => !u.IsDeleted)
            .SingleOrDefaultAsync(u => u.PhoneNumber == number, cancellationToken);
    }

    public async Task<User?> FindByEmailOrPhoneAsync(string? email, string? phone,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email?.ToUpper();
        return await userManager.Users
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u =>
                    u.NormalizedEmail == normalizedEmail || u.PhoneNumber == phone,
                cancellationToken: cancellationToken);
    }

    public async Task<IdentityResult> DeleteAsync(User user)
    {
        return await userManager.DeleteAsync(user);
    }

    public async Task<IList<Claim>> GetClaimsAsync(User user)
    {
        return await userManager.GetClaimsAsync(user);
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        return await userManager.UpdateAsync(user);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await userManager.CheckPasswordAsync(user, password);
    }

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
}