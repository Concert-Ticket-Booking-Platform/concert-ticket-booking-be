using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertTicket.Infrastructure.Persistence.Configurations
{
    public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
    {
        public void Configure(EntityTypeBuilder<TicketCategory> builder)
        {
            builder.ToTable("ticket_categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Price)
                .HasColumnName("price")
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(x => x.TotalQuantity)
                .HasColumnName("total_quantity");

            builder.Property(x => x.AvailableQuantity)
                .HasColumnName("available_quantity");

            builder.Property(x => x.ReservedQuantity)
                .HasColumnName("reserved_quantity");

            builder.Property(x => x.SoldQuantity)
                .HasColumnName("sold_quantity");

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            builder.HasOne(x => x.Concert)
                .WithMany(x => x.TicketCategories)
                .HasForeignKey(x => x.ConcertId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.ConcertId);
        }
    }
}
