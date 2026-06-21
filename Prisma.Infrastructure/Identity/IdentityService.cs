using Microsoft.AspNetCore.Identity;
using Prisma.Application.Abstractions.Services;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;

    public IdentityService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public Task<IdentityResult> CreateAsync(User user, string password)
        => _userManager.CreateAsync(user, password);

    public Task<IdentityResult> AddToRoleAsync(User user, string role)
        => _userManager.AddToRoleAsync(user, role);

    public Task<User?> FindByEmailAsync(string email)
        => _userManager.FindByEmailAsync(email);

    public Task<bool> CheckPasswordAsync(User user, string password)
        => _userManager.CheckPasswordAsync(user, password);

    public Task<string> GenerateEmailConfirmationTokenAsync(User user)
        => _userManager.GenerateEmailConfirmationTokenAsync(user);
}