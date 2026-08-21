using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicket.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Concert> Concerts { get; }

        DbSet<TicketCategory> TicketCategories { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
