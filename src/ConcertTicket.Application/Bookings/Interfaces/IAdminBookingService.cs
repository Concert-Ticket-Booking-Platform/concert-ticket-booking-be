using ConcertTicket.Application.Bookings.DTOs;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Bookings.Interfaces;

public interface IAdminBookingService
{
    Task<PagedResult<AdminBookingDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        BookingStatus? status = null,
        Guid? userId = null,
        Guid? concertId = null,
        string? bookingCode = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);

    Task<AdminBookingDetailDto> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);
}
