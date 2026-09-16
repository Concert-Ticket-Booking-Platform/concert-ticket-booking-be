namespace ConcertTicket.Application.Concerts.DTOs;

public sealed record AdminConcertDto(
    Guid Id,
    string ConcertName,
    string? Description,
    string? ImageUrl,
    string Venue,
    DateTimeOffset EventDate,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
