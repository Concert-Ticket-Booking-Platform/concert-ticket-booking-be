using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Vouchers.DTOs;

public sealed record UpdateVoucherRequest(
    string Code,
    string Name,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    int UsageLimit,
    DateTimeOffset StartsAt,
    DateTimeOffset ExpiresAt);
