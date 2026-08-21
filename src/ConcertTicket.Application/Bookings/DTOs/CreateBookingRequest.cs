namespace ConcertTicket.Application.Bookings.DTOs;

public sealed record CreateBookingRequest(
    Guid ConcertId,
    Guid TicketCategoryId,
    int Quantity,
    string? VoucherCode);
