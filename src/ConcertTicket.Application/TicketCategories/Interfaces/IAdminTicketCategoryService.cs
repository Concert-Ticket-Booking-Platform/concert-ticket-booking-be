using ConcertTicket.Application.TicketCategories.DTOs;

namespace ConcertTicket.Application.TicketCategories.Interfaces;

public interface IAdminTicketCategoryService
{
    Task<AdminTicketCategoryDto> CreateAsync(
        Guid concertId,
        CreateTicketCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminTicketCategoryDto?> GetByIdAsync(
        Guid ticketCategoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminTicketCategoryDto>> GetByConcertIdAsync(
        Guid concertId,
        CancellationToken cancellationToken = default);

    Task<AdminTicketCategoryDto> UpdateAsync(
        Guid ticketCategoryId,
        UpdateTicketCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminTicketCategoryDto> UpdateStatusAsync(
        Guid ticketCategoryId,
        UpdateTicketCategoryStatusRequest request,
        CancellationToken cancellationToken = default);
}
