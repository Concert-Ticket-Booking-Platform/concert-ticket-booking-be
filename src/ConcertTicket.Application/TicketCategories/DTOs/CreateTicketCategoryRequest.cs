namespace ConcertTicket.Application.TicketCategories.DTOs;

public sealed record CreateTicketCategoryRequest(
    Guid ConcertId,
    string Name,
    decimal Price,
    int TotalQuantity);
