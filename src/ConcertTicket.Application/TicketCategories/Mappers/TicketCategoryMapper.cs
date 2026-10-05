using ConcertTicket.Application.TicketCategories.DTOs;
using ConcertTicket.Domain.Entities;

namespace ConcertTicket.Application.TicketCategories.Mappers;

public static class TicketCategoryMapper
{
    public static AdminTicketCategoryDto ToAdminDto(
        TicketCategory category)
    {
        return new AdminTicketCategoryDto(
            category.Id,
            category.ConcertId,
            category.Concert.ConcertName,
            category.Name,
            category.Price,
            category.TotalQuantity,
            category.AvailableQuantity,
            category.ReservedQuantity,
            category.SoldQuantity,
            category.Status.ToString(),
            category.CreatedAt,
            category.UpdatedAt);
    }
}
