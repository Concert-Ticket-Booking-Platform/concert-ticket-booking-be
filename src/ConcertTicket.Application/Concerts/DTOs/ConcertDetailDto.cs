namespace ConcertTicket.Application.Concerts.DTOs;

public sealed record ConcertDetailDto(
    Guid Id,
    string ConcertName,
    string? Description,
    string Venue,
    DateTimeOffset EventDate,
    string Status,
    IReadOnlyList<TicketCategoryDto> TicketCategories);

public sealed record TicketCategoryDto(
    Guid Id,
    string Name,
    decimal Price,
    int AvailableQuantity);
