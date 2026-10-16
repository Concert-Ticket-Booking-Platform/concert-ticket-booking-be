using ConcertTicket.Application.Bookings.DTOs;
using ConcertTicket.Domain.Entities;

namespace ConcertTicket.Application.Bookings.Mappers;

public static class AdminBookingMapper
{
    public static AdminBookingDto ToDto(Booking booking)
    {
        return new AdminBookingDto(
            Id: booking.Id,
            BookingCode: booking.BookingCode,
            UserId: booking.UserId,
            Username: booking.User.Username,
            ConcertId: booking.ConcertId,
            ConcertName: booking.Concert.ConcertName,
            TotalAmount: booking.TotalAmount,
            DiscountAmount: booking.DiscountAmount,
            FinalAmount: booking.FinalAmount,
            Status: booking.Status.ToString(),
            TotalTicketQuantity: booking.BookingItems.Sum(x => x.Quantity),
            CreatedAt: booking.CreatedAt,
            ExpiresAt: booking.ExpiresAt,
            CompletedAt: booking.CompletedAt
        );
    }

    public static AdminBookingDetailDto ToDetailDto(Booking booking)
    {
        return new AdminBookingDetailDto(
            Id: booking.Id,
            BookingCode: booking.BookingCode,
            UserId: booking.UserId,
            Username: booking.User.Username,
            ConcertId: booking.ConcertId,
            ConcertName: booking.Concert.ConcertName,
            TotalAmount: booking.TotalAmount,
            DiscountAmount: booking.DiscountAmount,
            FinalAmount: booking.FinalAmount,
            Status: booking.Status.ToString(),
            CreatedAt: booking.CreatedAt,
            UpdatedAt: booking.UpdatedAt,
            ExpiresAt: booking.ExpiresAt,
            CompletedAt: booking.CompletedAt,

            Items: booking.BookingItems
                .Select(item => new AdminBookingItemDto(
                    Id: item.Id,
                    TicketCategoryId: item.TicketCategoryId,
                    TicketCategoryName: item.TicketCategory.Name,
                    Quantity: item.Quantity,
                    UnitPrice: item.UnitPrice,
                    Subtotal: item.Subtotal,
                    CreatedAt: item.CreatedAt
                ))
                .ToList(),

            Payments: booking.PaymentTransactions
                .OrderByDescending(payment => payment.CreatedAt)
                .Select(payment => new AdminBookingPaymentDto(
                    Id: payment.Id,
                    Amount: payment.Amount,
                    Status: payment.Status.ToString(),
                    OrderCode: payment.OrderCode,
                    Provider: payment.Provider.ToString(),
                    TransactionReference: payment.TransactionReference,
                    PaidAt: payment.PaidAt,
                    ExpiredAt: payment.ExpiredAt,
                    CreatedAt: payment.CreatedAt,
                    UpdatedAt: payment.UpdatedAt
                ))
                .ToList()
        );
    }
}
