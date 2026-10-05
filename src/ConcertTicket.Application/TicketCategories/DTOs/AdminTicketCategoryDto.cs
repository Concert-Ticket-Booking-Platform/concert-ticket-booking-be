namespace ConcertTicket.Application.TicketCategories.DTOs;

public sealed record AdminTicketCategoryDto(
    Guid Id,
    Guid ConcertId,
    string ConcertName,
    string Name,
    decimal Price,
    int TotalQuantity,
    int AvailableQuantity,
    int ReservedQuantity,
    int SoldQuantity,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
