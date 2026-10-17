using ConcertTicket.Application.Bookings.DTOs;
using ConcertTicket.Application.Bookings.Interfaces;
using ConcertTicket.Application.Bookings.Mappers;
using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Bookings.Services;

public sealed class AdminBookingService : IAdminBookingService
{
    private readonly IApplicationDbContext _dbContext;

    public AdminBookingService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminBookingDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        BookingStatus? status = null,
        Guid? userId = null,
        Guid? concertId = null,
        string? bookingCode = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            throw new ArgumentException("Page must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100.");

        if (status.HasValue && !Enum.IsDefined(status.Value))
            throw new ArgumentException("Invalid booking status.");

        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value > toDate.Value)
        {
            throw new ArgumentException(
                "FromDate must be earlier than or equal to ToDate.");
        }

        var query = _dbContext.Bookings
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (concertId.HasValue)
        {
            query = query.Where(x => x.ConcertId == concertId.Value);
        }

        if (!string.IsNullOrWhiteSpace(bookingCode))
        {
            var code = bookingCode.Trim();

            query = query.Where(x => x.BookingCode.Contains(code));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(
                x => x.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(
                x => x.CreatedAt <= toDate.Value);
        }

        // Đếm sau khi áp dụng filter, trước khi phân trang.
        var totalCount = await query.CountAsync(cancellationToken);

        var bookings = await query
            .Include(x => x.User)
            .Include(x => x.Concert)
            .Include(x => x.BookingItems)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = bookings
            .Select(AdminBookingMapper.ToDto)
            .ToList();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        return new PagedResult<AdminBookingDto>(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        );
    }

    public async Task<AdminBookingDetailDto> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Concert)
            .Include(x => x.BookingItems)
                .ThenInclude(x => x.TicketCategory)
            .Include(x => x.PaymentTransactions)
            .FirstOrDefaultAsync(
                x => x.Id == bookingId,
                cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                $"Booking '{bookingId}' was not found.");
        }

        return AdminBookingMapper.ToDetailDto(booking);
    }
}
