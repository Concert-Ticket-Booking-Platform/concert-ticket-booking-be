namespace ConcertTicket.Application.Concerts.DTOs;

public sealed record CreateConcertRequest(
    string ConcertName,
    string? Description,
    string? ImageUrl,
    string Venue,
    DateTimeOffset EventDate);
