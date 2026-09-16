namespace ConcertTicket.Application.Concerts.DTOs;

public sealed record UpdateConcertRequest(
    string ConcertName,
    string? Description,
    string? ImageUrl,
    string Venue,
    DateTimeOffset EventDate);
