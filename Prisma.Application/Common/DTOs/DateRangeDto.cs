using Ardalis.Result;
using Prisma.Domain.ValueObjects.EnrollmentDomain;

namespace Prisma.Application.Common.DTOs;

public record DateRangeDto(DateTimeOffset StartDate, DateTimeOffset EndDate, TimeSpan Duration);

public static class DateRangeDtoExtensions
{
    public static DateRangeDto ToDto(this DateRange dateRange)
    {
        ArgumentNullException.ThrowIfNull(dateRange, nameof(dateRange));
        return new DateRangeDto(dateRange.StartDate, dateRange.EndDate, dateRange.Duration);
    }

    public static Result<DateRange> ToDomain(this DateRangeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));
        return DateRange.Create(dto.StartDate, dto.EndDate);
    }
}
