using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
    {
        public void Configure(EntityTypeBuilder<BookingItem> builder)
        {
            builder.ToTable("booking_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(12, 2);

            builder.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasPrecision(12, 2);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.HasOne(x => x.Booking)
                .WithMany(x => x.BookingItems)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TicketCategory)
                .WithMany(x => x.BookingItems)
                .HasForeignKey(x => x.TicketCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.BookingId);
            builder.HasIndex(x => x.TicketCategoryId);
        }
    }
}
