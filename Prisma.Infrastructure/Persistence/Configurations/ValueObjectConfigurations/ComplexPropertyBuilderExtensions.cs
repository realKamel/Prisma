using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prisma.Domain.ValueObjects;
using Prisma.Domain.ValueObjects.AssessmentDomain;
using Prisma.Domain.ValueObjects.Common;
using Prisma.Domain.ValueObjects.ContentDomain;
using Prisma.Domain.ValueObjects.EnrollmentDomain;
using Prisma.Domain.ValueObjects.UserDomain;

namespace Prisma.Infrastructure.Persistence.Configurations.ValueObjectConfigurations;

internal static class ComplexPropertyBuilderExtensions
{
    public static ComplexPropertyBuilder<FullName> ConfigureFullName<TEntity>(
        this ComplexPropertyBuilder<FullName> builder,
        string prefix = "FullName") where TEntity : class
    {
        builder.Property(f => f.FirstName)
            .HasColumnName($"{prefix}_FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.SecondName)
            .HasColumnName($"{prefix}_SecondName")
            .HasMaxLength(100)
            .IsRequired();

        return builder;
    }

    public static ComplexPropertyBuilder<Money> ConfigureMoney<TEntity>(
        this ComplexPropertyBuilder<Money> builder,
        string prefix = "Money") where TEntity : class
    {
        builder.Property(m => m.Amount)
            .HasColumnName($"{prefix}_Amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(m => m.Currency)
            .HasColumnName($"{prefix}_Currency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        return builder;
    }

    public static ComplexPropertyBuilder<EmailAddress> ConfigureEmailAddress<TEntity>(
        this ComplexPropertyBuilder<EmailAddress> builder,
        string columnName = "Email") where TEntity : class
    {
        builder.Property(e => e.Value)
            .HasColumnName(columnName)
            .HasMaxLength(255)
            .IsRequired();

        return builder;
    }

    public static ComplexPropertyBuilder<DateRange> ConfigureDateRange<TEntity>(
        this ComplexPropertyBuilder<DateRange> builder,
        string columnName = "DateRange") where TEntity : class
    {
        builder.Property(dr => dr.StartDate)
            .HasColumnName($"{columnName}_StartDate");

        builder.Property(dr => dr.EndDate)
            .HasColumnName($"{columnName}_EndDate");
        return builder;
    }

    public static ComplexPropertyBuilder<Percentage> ConfigurePercentage<TEntity>(
        this ComplexPropertyBuilder<Percentage> builder,
        string columnName = "Percentage") where TEntity : class
    {
        builder.Property(p => p.Value)
            .HasColumnName(columnName)
            .HasPrecision(5, 2)
            .IsRequired();

        return builder;
    }

    /// <summary>
    /// Configures the Slug complex property with Unicode/NVARCHAR support for Arabic characters.
    /// </summary>
    public static ComplexPropertyBuilder<Slug> ConfigureSlug<TEntity>(
        this ComplexPropertyBuilder<Slug> builder,
        string columnName = "Slug") where TEntity : class
    {
        builder.Property(s => s.Value)
            .HasColumnName(columnName)
            .HasMaxLength(200)
            .IsUnicode(true)
            .IsRequired();

        return builder;
    }

    /// <summary>
    /// Configures the OptionKey complex property mapped as a single character (e.g. 'A', 'B', 'C').
    /// </summary>
    public static ComplexPropertyBuilder<OptionKey> ConfigureOptionKey<TEntity>(
        this ComplexPropertyBuilder<OptionKey> builder,
        string columnName = "OptionKey") where TEntity : class
    {
        builder.Property(o => o.Value)
            .HasColumnName(columnName)
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsRequired();

        return builder;
    }

    /// <summary>
    /// Configures the PassingScore complex property, flattening its nested Percentage value.
    /// </summary>
    public static ComplexPropertyBuilder<PassingScore> ConfigurePassingScore<TEntity>(
        this ComplexPropertyBuilder<PassingScore> builder,
        string columnName = "PassingScorePercentage") where TEntity : class
    {
        builder.ComplexProperty(p => p.Value, percentageBuilder =>
        {
            percentageBuilder.Property(p => p.Value)
                .HasColumnName(columnName)
                .HasPrecision(5, 2)
                .IsRequired();
        });

        return builder;
    }


    public static ComplexPropertyBuilder<Score> ConfigureScore<TEntity>(
        this ComplexPropertyBuilder<Score> builder,
        string prefix = "Score") where TEntity : class
    {
        builder.Property(g => g.AchievedPoints)
            .HasColumnName($"{prefix}_AchievedPoints")
            .HasPrecision(7, 2)
            .IsRequired();

        builder.Property(g => g.MaxPoints)
            .HasColumnName($"{prefix}_MaxPoints")
            .HasPrecision(7, 2)
            .IsRequired();

        return builder;
    }

    public static ComplexPropertyBuilder<PhoneNumber> ConfigurePhoneNumber<TEntity>(
        this ComplexPropertyBuilder<PhoneNumber> builder,
        string columnName = "MobilePhoneNumber",
        int maxLength = 20) where TEntity : class
    {
        builder.Property(p => p.Value)
            .HasColumnName(columnName)
            .HasMaxLength(maxLength)
            .IsRequired();

        return builder;
    }
}