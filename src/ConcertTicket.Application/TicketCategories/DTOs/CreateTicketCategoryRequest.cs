namespace ConcertTicket.Application.TicketCategories.DTOs;

public sealed record CreateTicketCategoryRequest(
    string Name,
    decimal Price,
    int TotalQuantity);
