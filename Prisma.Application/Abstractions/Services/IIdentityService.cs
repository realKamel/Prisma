using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Application.Abstractions.Services;

public interface IIdentityService
{
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> FindByPhoneNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailOrPhoneAsync(string? email, string? phone, CancellationToken cancellationToken = default);
    Task<IdentityResult> DeleteAsync(User user);
    Task<IList<Claim>> GetClaimsAsync(User user);
    Task<IdentityResult> UpdateAsync(User user);
    Task<IList<string>> GetRolesAsync(User user);
    Task<List<TUser>> GetUsers<TUser>(CancellationToken cancellationToken = default) where TUser : User;
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IdentityResult> AddClaimsAsync(User user, IEnumerable<Claim> claims);
    Task<IdentityResult> RemoveClaimsAsync(User user, IEnumerable<Claim> claims);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
}