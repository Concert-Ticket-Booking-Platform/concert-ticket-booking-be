using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConcertTicket.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }

        DbSet<Role> Roles { get; }

        DbSet<Concert> Concerts { get; }

        DbSet<TicketCategory> TicketCategories { get; }

        DbSet<Booking> Bookings { get; }

        DbSet<BookingItem> BookingItems { get; }

        DbSet<Voucher> Vouchers { get; }

        DbSet<VoucherUsage> VoucherUsages { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task<int> TryConsumeVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default);
    }
}
