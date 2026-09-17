using ConcertTicket.Application.Concerts.DTOs;

namespace ConcertTicket.Application.Concerts.Interfaces;

public interface IAdminConcertService
{
    Task<AdminConcertDto> CreateAsync(
        CreateConcertRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default);

    Task<AdminConcertDto?> GetByIdAsync(
        Guid concertId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminConcertDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AdminConcertDto> UpdateAsync(
        Guid concertId,
        UpdateConcertRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminConcertDto> UpdateStatusAsync(
        Guid concertId,
        UpdateConcertStatusRequest request,
        CancellationToken cancellationToken = default);
}
