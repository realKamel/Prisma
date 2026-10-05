using Ardalis.Result;
using Prisma.Domain.ValueObjects.ContentDomain;

namespace Prisma.Application.Common.DTOs;

public sealed record MoneyDto(decimal Amount, string Currency);

public static class MoneyDtoExtensions
{
    public static MoneyDto ToDto(this Money money)
    {
        ArgumentNullException.ThrowIfNull(money, nameof(money));
        return new MoneyDto(money.Amount, money.Currency);
    }

    public static Result<Money> ToDomain(this MoneyDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));
        return Money.Create(dto.Amount, dto.Currency);
    }
}