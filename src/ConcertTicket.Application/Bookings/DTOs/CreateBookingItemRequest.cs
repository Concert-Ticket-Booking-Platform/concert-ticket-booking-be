namespace ConcertTicket.Application.Bookings.DTOs;

public sealed record CreateBookingItemRequest(
    Guid TicketCategoryId,
    int Quantity);
