namespace ConcertTicket.Application.Concerts.DTOs;

public sealed record ConcertListItemDto(
    Guid Id,
    string ConcertName,
    string Venue,
    DateTimeOffset EventDate,
    string Status);