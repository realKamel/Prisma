using Microsoft.EntityFrameworkCore;

namespace Prisma.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // to apply all configuration for class that implements IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}