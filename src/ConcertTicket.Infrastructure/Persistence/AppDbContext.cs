using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Domain.Entities;
using ConcertTicket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConcertTicket.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IApplicationDbContext, IUnitOfWork
    {
        private IDbContextTransaction? _currentTransaction;


        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Concert> Concerts => Set<Concert>();
        public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction =
                await Database.BeginTransactionAsync(
                    cancellationToken);
        }

        public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                return;

            try
            {
                await SaveChangesAsync(cancellationToken);

                await _currentTransaction.CommitAsync(
                    cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(
                    cancellationToken);

                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                return;

            await _currentTransaction.RollbackAsync(
                cancellationToken);

            await _currentTransaction.DisposeAsync();

            _currentTransaction = null;
        }

        /// <summary>
        /// Attempts to consume a voucher by incrementing its UsedCount if it is active and has not reached its usage limit.
        /// </summary>
        public async Task<int> TryConsumeVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default)
        {
            return await Vouchers
                .Where(x =>
                    x.Id == voucherId &&
                    x.Status == VoucherStatus.Active &&
                    x.UsedCount < x.UsageLimit)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        x => x.UsedCount,
                        x => x.UsedCount + 1),
                    cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
