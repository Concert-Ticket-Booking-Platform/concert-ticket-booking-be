namespace ConcertTicket.Application.Bookings.Interfaces;

public interface IBookingExpirationService
{
    Task<int> ExpireBookingsAsync(
        CancellationToken cancellationToken = default);
}
