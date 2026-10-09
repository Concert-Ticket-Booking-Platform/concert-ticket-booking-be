using ConcertTicket.Application.Vouchers.DTOs;
using ConcertTicket.Domain.Entities;

namespace ConcertTicket.Application.Vouchers.Mappers;

public static class VoucherMapper
{
    public static AdminVoucherDto ToAdminDto(Voucher voucher)
    {
        return new AdminVoucherDto(
            voucher.Id,
            voucher.Code,
            voucher.Name,
            voucher.DiscountType.ToString(),
            voucher.DiscountValue,
            voucher.MaxDiscountAmount,
            voucher.UsageLimit,
            voucher.UsedCount,
            voucher.StartsAt,
            voucher.ExpiresAt,
            voucher.Status.ToString(),
            voucher.CreatedAt,
            voucher.UpdatedAt);
    }
}
