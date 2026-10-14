using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Vouchers.DTOs;
using ConcertTicket.Application.Vouchers.Interfaces;
using ConcertTicket.Application.Vouchers.Mappers;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Vouchers.Services;

public sealed class AdminVoucherService : IAdminVoucherService
{
    private readonly IApplicationDbContext _dbContext;

    public AdminVoucherService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminVoucherDto> CreateAsync(
        CreateVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateVoucher(
            request.Code,
            request.Name,
            request.DiscountType,
            request.DiscountValue,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.StartsAt,
            request.ExpiresAt);

        var now = DateTimeOffset.UtcNow;

        if (request.ExpiresAt <= now)
        {
            throw new ArgumentException(
                "Voucher expiration time must be in the future.");
        }

        var normalizedCode = NormalizeCode(request.Code);

        var codeExists = await _dbContext.Vouchers
            .AnyAsync(
                x => x.Code == normalizedCode, cancellationToken);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"Voucher code '{normalizedCode}' already exists.");
        }

        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            StartsAt = request.StartsAt,
            ExpiresAt = request.ExpiresAt,
            Status = VoucherStatus.Active,
            CreatedAt = now,
            UpdatedAt = null
        };

        _dbContext.Vouchers.Add(voucher);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VoucherMapper.ToAdminDto(voucher);
    }


    public async Task<PagedResult<AdminVoucherDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentException(
                "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException(
                "Page size must be between 1 and 100.");
        }

        var query = _dbContext.Vouchers.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var vouchers = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = vouchers
            .Select(VoucherMapper.ToAdminDto)
            .ToList();

        return new PagedResult<AdminVoucherDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }

    //public async Task<IReadOnlyList<AdminVoucherDto>> GetAllAsync(
    //    CancellationToken cancellationToken = default)
    //{
    //    var vouchers = await _dbContext.Vouchers
    //        .AsNoTracking()
    //        .OrderByDescending(x => x.CreatedAt)
    //        .ToListAsync(cancellationToken);

    //    return vouchers
    //        .Select(VoucherMapper.ToAdminDto)
    //        .ToList();
    //}

    public async Task<AdminVoucherDto> GetByIdAsync(
        Guid voucherId,
        CancellationToken cancellationToken = default)
    {
        var voucher = await _dbContext.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == voucherId,
                cancellationToken);

        if (voucher is null)
        {
            throw new KeyNotFoundException(
                $"Voucher '{voucherId}' was not found.");
        }

        return VoucherMapper.ToAdminDto(voucher);
    }

    public async Task<AdminVoucherDto> UpdateAsync(
        Guid voucherId,
        UpdateVoucherRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateVoucher(
            request.Code,
            request.Name,
            request.DiscountType,
            request.DiscountValue,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.StartsAt,
            request.ExpiresAt);

        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(
                x => x.Id == voucherId, cancellationToken);

        if (voucher is null)
        {
            throw new KeyNotFoundException(
                $"Voucher '{voucherId}' was not found.");
        }

        var normalizedCode = NormalizeCode(request.Code);

        // Check whether another voucher uses the same code.
        var duplicateExists = await _dbContext.Vouchers
            .AnyAsync(
                x => x.Id != voucherId &&
                     x.Code == normalizedCode, cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Voucher code '{normalizedCode}' already exists.");
        }

        if (request.UsageLimit < voucher.UsedCount)
        {
            throw new InvalidOperationException(
                "Usage limit cannot be less than the current used count.");
        }

        // Also check usage history because UsedCount can decrease
        // when a booking is cancelled or expires.
        var hasUsageHistory = await _dbContext.VoucherUsages
            .AnyAsync(
                x => x.VoucherId == voucherId, cancellationToken);

        var hasBeenUsed = voucher.UsedCount > 0 || hasUsageHistory;

        if (hasBeenUsed)
        {
            var discountTermsChanged =
                voucher.Code != normalizedCode ||
                voucher.DiscountType != request.DiscountType ||
                voucher.DiscountValue != request.DiscountValue ||
                voucher.MaxDiscountAmount != request.MaxDiscountAmount ||
                voucher.StartsAt != request.StartsAt ||
                voucher.ExpiresAt != request.ExpiresAt;

            if (discountTermsChanged)
            {
                throw new InvalidOperationException(
                    "Voucher code, discount terms, and validity period " +
                    "cannot be changed after the voucher has been used.");
            }
        }

        // Do not allow an update to turn an already expired voucher
        // into a voucher with a new validity period.
        if (voucher.Status == VoucherStatus.Expired)
        {
            throw new InvalidOperationException(
                "An expired voucher cannot be updated.");
        }

        voucher.Code = normalizedCode;
        voucher.Name = request.Name.Trim();
        voucher.DiscountType = request.DiscountType;
        voucher.DiscountValue = request.DiscountValue;
        voucher.MaxDiscountAmount = request.MaxDiscountAmount;
        voucher.UsageLimit = request.UsageLimit;
        voucher.StartsAt = request.StartsAt;
        voucher.ExpiresAt = request.ExpiresAt;
        voucher.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VoucherMapper.ToAdminDto(voucher);
    }

    public async Task<AdminVoucherDto> UpdateStatusAsync(
        Guid voucherId,
        UpdateVoucherStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Enum.IsDefined(typeof(VoucherStatus), request.Status))
        {
            throw new ArgumentException("Invalid voucher status.");
        }

        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(
                x => x.Id == voucherId,
                cancellationToken);

        if (voucher is null)
        {
            throw new KeyNotFoundException(
                $"Voucher '{voucherId}' was not found.");
        }

        // An expired voucher is terminal.
        if (voucher.Status == VoucherStatus.Expired)
        {
            if (request.Status == VoucherStatus.Expired)
            {
                return VoucherMapper.ToAdminDto(voucher);
            }

            throw new InvalidOperationException(
                "An expired voucher cannot be reactivated.");
        }

        // Idempotent request: nothing to change.
        if (voucher.Status == request.Status)
        {
            return VoucherMapper.ToAdminDto(voucher);
        }

        var now = DateTimeOffset.UtcNow;

        switch (request.Status)
        {
            case VoucherStatus.Active:
                {
                    if (now < voucher.StartsAt)
                    {
                        throw new InvalidOperationException(
                            "A voucher cannot be activated before its start time.");
                    }

                    if (now >= voucher.ExpiresAt)
                    {
                        throw new InvalidOperationException(
                            "An expired voucher cannot be activated.");
                    }

                    if (voucher.UsedCount >= voucher.UsageLimit)
                    {
                        throw new InvalidOperationException(
                            "A voucher with no remaining uses cannot be activated.");
                    }

                    voucher.Status = VoucherStatus.Active;
                    break;
                }

            case VoucherStatus.Inactive:
                {
                    voucher.Status = VoucherStatus.Inactive;
                    break;
                }

            case VoucherStatus.Expired:
                {
                    if (now < voucher.ExpiresAt)
                    {
                        throw new InvalidOperationException(
                            "A voucher can only be marked as expired " +
                            "after its expiration time.");
                    }

                    voucher.Status = VoucherStatus.Expired;
                    break;
                }

            default:
                throw new ArgumentException("Invalid voucher status.");
        }

        voucher.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return VoucherMapper.ToAdminDto(voucher);
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static void ValidateVoucher(
        string code,
        string name,
        DiscountType discountType,
        decimal discountValue,
        decimal? maxDiscountAmount,
        int usageLimit,
        DateTimeOffset startsAt,
        DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Voucher code is required.");
        }

        if (code.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Voucher code cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Voucher name is required.");
        }

        if (name.Trim().Length > 255)
        {
            throw new ArgumentException(
                "Voucher name cannot exceed 255 characters.");
        }

        if (!Enum.IsDefined(typeof(DiscountType), discountType))
        {
            throw new ArgumentException(
                "Invalid discount type.");
        }

        if (discountValue <= 0)
        {
            throw new ArgumentException(
                "Discount value must be greater than zero.");
        }

        if (discountType == DiscountType.Percentage &&
            discountValue > 100)
        {
            throw new ArgumentException(
                "Percentage discount cannot exceed 100.");
        }

        if (maxDiscountAmount.HasValue &&
            maxDiscountAmount.Value <= 0)
        {
            throw new ArgumentException(
                "Maximum discount amount must be greater than zero.");
        }

        if (usageLimit <= 0)
        {
            throw new ArgumentException(
                "Usage limit must be greater than zero.");
        }

        if (expiresAt <= startsAt)
        {
            throw new ArgumentException(
                "Expiration time must be later than start time.");
        }
    }
}
