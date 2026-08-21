using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Concerts.DTOs;
using ConcertTicket.Application.Concerts.Interfaces;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Concerts.Services;

public sealed class ConcertService : IConcertService
{
    private readonly IApplicationDbContext _dbContext;

    public ConcertService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ConcertListItemDto>>
        GetPublishedConcertsAsync(
            CancellationToken cancellationToken)
    {
        return await _dbContext.Concerts
            .AsNoTracking()
            .Where(x => x.Status == ConcertStatus.Published)
            .OrderBy(x => x.EventDate)
            .Select(x => new ConcertListItemDto(
                x.Id,
                x.ConcertName,
                x.Venue,
                x.EventDate,
                x.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConcertDetailDto?>
        GetConcertByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.Concerts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ConcertDetailDto(
                x.Id,
                x.ConcertName,
                x.Description,
                x.Venue,
                x.EventDate,
                x.Status.ToString(),
                x.TicketCategories
                    .Select(category => new TicketCategoryDto(
                        category.Id,
                        category.Name,
                        category.Price,
                        category.AvailableQuantity))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
