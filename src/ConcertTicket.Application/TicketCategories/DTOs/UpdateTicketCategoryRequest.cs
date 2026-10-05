namespace ConcertTicket.Application.TicketCategories.DTOs;

public sealed record UpdateTicketCategoryRequest(
    string Name,
    decimal Price,
    int TotalQuantity);
