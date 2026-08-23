namespace ConcertTicket.Application.Vouchers.DTOs;

public sealed record VoucherDto(
    Guid Id,
    string Code,
    string Name,
    string DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    int UsageLimit,
    int UsedCount,
    DateTimeOffset StartsAt,
    DateTimeOffset ExpiresAt,
    string Status);
