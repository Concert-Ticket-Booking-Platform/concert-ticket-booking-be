using ConcertTicket.Application.Concerts.DTOs;
using ConcertTicket.Domain.Entities;

namespace ConcertTicket.Application.Concerts.Mappers;

public static class ConcertMapper
{
    public static AdminConcertDto ToAdminDto(Concert concert)
    {
        return new AdminConcertDto(
            concert.Id,
            concert.ConcertName,
            concert.Description,
            concert.ImageUrl,
            concert.Venue,
            concert.EventDate,
            concert.Status.ToString(),
            concert.Creator.Username,
            concert.CreatedAt,
            concert.UpdatedAt);
    }
}
