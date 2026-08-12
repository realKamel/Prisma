using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Prisma.Domain.Entities.UserAggregate;

namespace Prisma.Infrastructure.Persistence.Configurations;

internal class TeacherStudentConfiguration: IEntityTypeConfiguration<TeacherStudent>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TeacherStudent> builder)
    {
        builder.HasKey(ts => ts.Id);

        builder.HasOne(ts => ts.Teacher)
            .WithMany(t => t.TeacherStudents)
            .HasForeignKey(ts => ts.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Student)
            .WithMany(s => s.TeacherStudents)
            .HasForeignKey(ts => ts.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(ts => !ts.IsDeleted);

        builder.HasIndex(ts => new { ts.TeacherId, ts.StudentId })
           .IsUnique()
           .HasFilter("\"IsDeleted\" = false");
    }
}
