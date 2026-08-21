using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BookingCode)
                .HasColumnName("booking_code")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(x => x.BookingCode)
                .IsUnique();

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(12, 2);

            builder.Property(x => x.DiscountAmount)
                .HasColumnName("discount_amount")
                .HasPrecision(12, 2);

            builder.Property(x => x.FinalAmount)
                .HasColumnName("final_amount")
                .HasPrecision(12, 2);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.ExpiresAt)
                .HasColumnName("expires_at");

            builder.Property(x => x.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(x => x.IdempotencyKey)
                .HasColumnName("idempotency_key")
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.UserId,
                x.IdempotencyKey
            })
            .IsUnique();

            builder.HasOne(x => x.User)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Concert)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.ConcertId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ConcertId);
            builder.HasIndex(x => x.Status);
        }
    }
}
