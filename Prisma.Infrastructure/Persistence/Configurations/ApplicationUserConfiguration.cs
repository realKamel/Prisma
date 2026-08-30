//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Prisma.Infrastructure.Identity.Entities;

//namespace Prisma.Infrastructure.Persistence.Configurations;

//internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
//{
//    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
//    {
//        builder.HasKey(b => b.Id);
//        builder.HasQueryFilter(u => !u.IsDeleted);
//    }
//}
