using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Reserve tickets for a specific ticket category.
    /// </summary>
    public async Task<int> ReserveAsync(
        Guid ticketCategoryId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TicketCategories
            .Where(x =>
                x.Id == ticketCategoryId &&
                x.Status == TicketCategoryStatus.Active &&
                x.AvailableQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.AvailableQuantity,
                        x => x.AvailableQuantity - quantity)
                    .SetProperty(
                        x => x.ReservedQuantity,
                        x => x.ReservedQuantity + quantity),
                cancellationToken);
    }

    /// <summary>
    /// Release reserved tickets for a specific ticket category.
    /// </summary>
    public async Task<int> ReleaseAsync(
        Guid ticketCategoryId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TicketCategories
            .Where(x =>
                x.Id == ticketCategoryId &&
                x.ReservedQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.AvailableQuantity,
                        x => x.AvailableQuantity + quantity)
                    .SetProperty(
                        x => x.ReservedQuantity,
                        x => x.ReservedQuantity - quantity),
                cancellationToken);
    }

    /// <summary>
    /// Confirm the sale of reserved tickets for a specific ticket category.
    /// </summary>
    public async Task<int> ConfirmAsync(
        Guid ticketCategoryId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TicketCategories
            .Where(x =>
                x.Id == ticketCategoryId &&
                x.ReservedQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.ReservedQuantity,
                        x => x.ReservedQuantity - quantity)
                    .SetProperty(
                        x => x.SoldQuantity,
                        x => x.SoldQuantity + quantity),
                cancellationToken);
    }
}
