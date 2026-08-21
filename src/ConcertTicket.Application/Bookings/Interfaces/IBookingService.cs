using ConcertTicket.Application.Bookings.DTOs;

namespace ConcertTicket.Application.Bookings.Interfaces;

public interface IBookingService
{
    Task<CreateBookingResponse> CreateAsync(
        Guid userId,
        string idempotencyKey,
        CreateBookingRequest request,
        CancellationToken cancellationToken);

    Task<BookingDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
