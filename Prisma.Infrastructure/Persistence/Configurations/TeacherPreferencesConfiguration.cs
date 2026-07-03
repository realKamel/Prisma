using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.Entities;

namespace Prisma.Infrastructure.Persistence.Configurations;

public class TeacherPreferencesConfiguration : IEntityTypeConfiguration<TeacherPreferences>
{
    public void Configure(EntityTypeBuilder<TeacherPreferences> builder)
    {

        builder.HasKey(p => p.Id); 

        builder.Property(p => p.Id)
            .ValueGeneratedNever(); 

        builder.Property(p => p.AccentColor)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.HasOne(p => p.Teacher)
            .WithOne(t => t.Preferences)
            .HasForeignKey<TeacherPreferences>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
