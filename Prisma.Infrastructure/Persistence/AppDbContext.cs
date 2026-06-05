using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence;


public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // to apply all configuration for class that implements IEntityTypeConfiguration
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}