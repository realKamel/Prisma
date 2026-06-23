using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public Task<IdentityResult> CreateAsync(User user, string password)
        => userManager.CreateAsync(user, password);

    public Task<IdentityResult> AddToRoleAsync(User user, string role)
        => userManager.AddToRoleAsync(user, role);

    public Task<User?> FindByEmailAsync(string email)
        => userManager.FindByEmailAsync(email);

    public Task<User?> FindByIdAsync(string userId) => userManager.FindByIdAsync(userId);

    public Task<User?> FindByPhoneNumberAsync(string number)
        => userManager.Users.SingleOrDefaultAsync(u => u.PhoneNumber == number);

    public Task<User?> FindByNameOrEmailAsync(string email, string phone)
    {
        var normalizedEmail = email.ToUpper();
        return userManager.Users
            .FirstOrDefaultAsync(u =>
                u.NormalizedEmail == normalizedEmail || u.PhoneNumber == phone);
    }

    public Task<IdentityResult> DeleteAsync(User user)
    {
        return userManager.DeleteAsync(user);
    }

    public Task<bool> CheckPasswordAsync(User user, string password)
        => userManager.CheckPasswordAsync(user, password);

    public Task<string> GenerateEmailConfirmationTokenAsync(User user)
        => userManager.GenerateEmailConfirmationTokenAsync(user);
}