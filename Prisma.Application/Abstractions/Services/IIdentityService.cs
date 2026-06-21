using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Abstractions.Services;

public interface IIdentityService
{
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<User?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
}