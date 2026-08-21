using ConcertTicket.Application.Concerts.DTOs;

namespace ConcertTicket.Application.Concerts.Interfaces;

public interface IConcertService
{
    Task<IReadOnlyList<ConcertListItemDto>> GetPublishedConcertsAsync(
        CancellationToken cancellationToken);

    Task<ConcertDetailDto?> GetConcertByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}