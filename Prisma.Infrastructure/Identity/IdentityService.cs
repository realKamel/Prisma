using System.Security.Claims;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prisma.Application.Abstractions.Services;
using Prisma.Application.Common.Constants;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        // Mapping
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

    public async Task<List<TUser>> GetUsers<TUser>(CancellationToken cancellationToken)
        where TUser : User
    {
        return await userManager.Users.OfType<TUser>().ToListAsync(cancellationToken);
    }

    public async Task<User?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var query = userManager
            .Users.Include(u => u.Roles)
            .ThenInclude(x => x.Role)
            .Include(c => c.Claims)
            .AsSplitQuery();

        return await query.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> FindByIdAsync(
        Guid userId,
        bool isTracking,
        CancellationToken cancellationToken = default
    )
    {
        var query = userManager
            .Users.Include(u => u.Roles)
            .ThenInclude(x => x.Role)
            .Include(c => c.Claims)
            .AsSplitQuery();

        if (!isTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> FindByPhoneNumberAsync(
        string number,
        CancellationToken cancellationToken
    )
    {
        return await userManager.Users.SingleOrDefaultAsync(
            u => u.PhoneNumber == number,
            cancellationToken
        );
    }

    public async Task<User?> FindByEmailOrPhoneAsync(
        string? email,
        string? phone,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var normalizedEmail = !string.IsNullOrWhiteSpace(email)
            ? userManager.NormalizeEmail(email)
            : null;

        return await userManager
            .Users.AsSplitQuery()
            .Include(u => u.Claims)
            .Include(u => u.Roles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(
                u =>
                    (normalizedEmail != null && u.NormalizedEmail == normalizedEmail)
                    || (!string.IsNullOrWhiteSpace(phone) && u.PhoneNumber == phone),
                cancellationToken: cancellationToken
            );
    }

    public async Task<IdentityResult> DeleteAsync(User user) => await userManager.DeleteAsync(user);

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

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(
        User user,
        string token,
        string newPassword
    )
    {
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }


    /// <summary>
    /// Checks whether a user possesses all specified permissions.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permissions">The list of permission strings to check against.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// <see cref="Result.Success()"/> if the user has all requested permissions;
    /// <see cref="Result.NotFound()"/> if the user does not exist;
    /// <see cref="Result.Unauthorized()"/> otherwise.
    /// </returns>
    public async Task<Result> HasPermissionsAsync(Guid userId, IReadOnlyList<string>? permissions,
        CancellationToken cancellationToken = default)
    {
        if (permissions is null || permissions.Count == 0)
        {
            return Result.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result.NotFound();
        }

        var claims = await userManager.GetClaimsAsync(user);

        var userPermissions = claims
            .Where(c => c.Type == AppClaims.PermissionsClaim)
            .Select(c => c.Value)
            .ToHashSet();

        // Ensures user holds EVERY requested permission in the input list
        return permissions.All(userPermissions.Contains) ? Result.Success() : Result.Unauthorized();
    }
}