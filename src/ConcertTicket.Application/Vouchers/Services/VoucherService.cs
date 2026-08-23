using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Vouchers.Interfaces;
using ConcertTicket.Application.Vouchers.DTOs;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Vouchers.Services;

public sealed class VoucherService : IVoucherService
{
    private readonly IApplicationDbContext _dbContext;

    public VoucherService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Voucher?> GetValidVoucherAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await _dbContext.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Code == code &&
                    x.Status == VoucherStatus.Active &&
                    x.StartsAt <= now &&
                    x.ExpiresAt > now &&
                    x.UsedCount < x.UsageLimit,
                cancellationToken);
    }

    public decimal CalculateDiscount(
        Voucher voucher,
        decimal subtotal)
    {
        if (subtotal <= 0)
            return 0;

        decimal discount;

        if (voucher.DiscountType == DiscountType.Percentage)
        {
            discount = subtotal *
                       voucher.DiscountValue /
                       100m;

            if (voucher.MaxDiscountAmount.HasValue)
            {
                discount = Math.Min(
                    discount,
                    voucher.MaxDiscountAmount.Value);
            }
        }
        else
        {
            discount = voucher.DiscountValue;
        }

        return Math.Min(
            Math.Max(discount, 0),
            subtotal);
    }

    public async Task<IReadOnlyList<VoucherDto>> GetAllVouchersAsync(
        CancellationToken cancellationToken)
    {
        var vouchers = await _dbContext.Vouchers
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return vouchers.Select(x => new VoucherDto(
            x.Id,
            x.Code,
            x.Name,
            x.DiscountType.ToString(),
            x.DiscountValue,
            x.MaxDiscountAmount,
            x.UsageLimit,
            x.UsedCount,
            x.StartsAt,
            x.ExpiresAt,
            x.Status.ToString()
        )).ToList();
    } 
}
