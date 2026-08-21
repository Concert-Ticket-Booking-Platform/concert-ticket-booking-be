using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class VoucherUsageConfiguration : IEntityTypeConfiguration<VoucherUsage>
    {
        public void Configure(EntityTypeBuilder<VoucherUsage> builder)
        {
            builder.ToTable("voucher_usages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UsedAt)
                .HasColumnName("used_at");

            builder.HasOne(x => x.User)
                .WithMany(x => x.VoucherUsages)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany(x => x.VoucherUsages)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Voucher)
                .WithMany(x => x.Usages)
                .HasForeignKey(x => x.VoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BookingId);
            builder.HasIndex(x => x.VoucherId);
            builder.HasIndex(x => x.UserId);
        }
    }
}
