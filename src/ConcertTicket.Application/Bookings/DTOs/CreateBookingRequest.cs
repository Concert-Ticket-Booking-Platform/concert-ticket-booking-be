namespace ConcertTicket.Application.Bookings.DTOs;

//public sealed record CreateBookingRequest(
//    Guid ConcertId,
//    Guid TicketCategoryId,
//    int Quantity,
//    string? VoucherCode);
public sealed record CreateBookingRequest(
    Guid ConcertId,
    IReadOnlyCollection<CreateBookingItemRequest> Items,
    string? VoucherCode);