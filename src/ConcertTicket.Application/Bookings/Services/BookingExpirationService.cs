using ConcertTicket.Application.Bookings.Interfaces;
using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConcertTicket.Application.Bookings.Services;

public sealed class BookingExpirationService
    : IBookingExpirationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingExpirationService> _logger;

    public BookingExpirationService(
        IApplicationDbContext dbContext,
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork,
        ILogger<BookingExpirationService> logger)
    {
        _dbContext = dbContext;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> ExpireBookingsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var bookingIds = await _dbContext.Bookings
            .AsNoTracking()
            .Where(x =>
                x.Status == BookingStatus.WaitingForPayment &&
                x.ExpiresAt != null &&
                x.ExpiresAt <= now)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var expiredCount = 0;

        foreach (var bookingId in bookingIds)
        {
            try
            {
                var expired =
                    await ExpireBookingAsync(
                        bookingId,
                        cancellationToken);

                if (expired)
                    expiredCount++;
            }
            catch (Exception ex)
            {
                // One failed booking should not stop
                // the worker from processing others.
                _logger.LogError(
                    ex,
                    "Failed to expire booking {BookingId}.",
                    bookingId);
            }
        }

        return expiredCount;
    }

    private async Task<bool> ExpireBookingAsync(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var booking = await _dbContext.Bookings
                .Include(x => x.BookingItems)
                .Include(x => x.VoucherUsages)
                .FirstOrDefaultAsync(
                    x => x.Id == bookingId,
                    cancellationToken);

            if (booking is null)
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                return false;
            }

            if (booking.Status !=
                BookingStatus.WaitingForPayment)
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                return false;
            }

            var now = DateTimeOffset.UtcNow;

            if (!booking.ExpiresAt.HasValue ||
                booking.ExpiresAt.Value > now)
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                return false;
            }

            foreach (var item in booking.BookingItems)
            {
                var released =
                    await _inventoryRepository.ReleaseAsync(
                        item.TicketCategoryId,
                        item.Quantity,
                        cancellationToken);

                if (released != 1)
                {
                    throw new InvalidOperationException(
                        $"Failed to release inventory for booking {booking.Id}.");
                }
            }

            foreach (var usage in booking.VoucherUsages)
            {
                var released =
                    await _dbContext.ReleaseVoucherUsageAsync(
                        usage.VoucherId,
                        cancellationToken);

                if (released != 1)
                {
                    throw new InvalidOperationException(
                        $"Failed to release voucher usage for booking {booking.Id}.");
                }
            }

            _dbContext.VoucherUsages.RemoveRange(
                booking.VoucherUsages);

            booking.Status = BookingStatus.Expired;
            booking.UpdatedAt = now;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }
}
